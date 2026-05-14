using System.IO;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SnowyRiver.Documents.Converter.Model;
using ConversionOptions = SnowyRiver.Documents.Converter.Abstractions.ConversionOptions;
using A = DocumentFormat.OpenXml.Drawing;

namespace SnowyRiver.Documents.Converter.Engines;

/// <summary>
/// 使用 DocumentFormat.OpenXml 解析 .docx 为 IR。
/// 支持：段落/run 的字体/字号/粗体/斜体/下划线/颜色/对齐、标题样式、表格（合并/列宽/边框/背景）、嵌入图片、嵌入图表的渲染缓存图、
/// 样式继承（styles.xml）、有序/无序列表（numbering.xml）、节级页面属性（sectPr）。
/// </summary>
internal static class WordReader
{
    public static IrDocument Read(Stream source, ConversionOptions options) => Read(source, options, null);

    public static IrDocument Read(Stream source, ConversionOptions options, SnowyRiver.Documents.Converter.Abstractions.ConversionDiagnostics? diagnostics)
    {
        using var ms = new MemoryStream();
        source.CopyTo(ms);
        ms.Position = 0;

        var ir = new IrDocument();
        using var doc = WordprocessingDocument.Open(ms, false);
        var main = doc.MainDocumentPart ?? throw new InvalidDataException("Word 文档缺少 MainDocumentPart。");
        var bodyEl = main.Document?.Body ?? throw new InvalidDataException("Word 文档缺少 Body。");

        ir.Title = options.Title ?? doc.PackageProperties.Title;
        ir.Author = options.Author ?? doc.PackageProperties.Creator;

        // 预加载样式表与编号表
        var ctx = new WordContext(main) { Diagnostics = diagnostics, EnableFields = options.EnableWordFields };

        // 预加载脚注/尾注（按 id 索引；忽略 separator/continuation 等系统注释）
        LoadNotes(main.FootnotesPart?.Footnotes, ir.Footnotes);
        LoadNotes(main.EndnotesPart?.Endnotes, ir.Endnotes);
        LoadComments(main.WordprocessingCommentsPart?.Comments, ir.Comments);

        // 应用文档级页面尺寸与边距（来自 body 末尾的 sectPr）
        var docSect = bodyEl.Elements<SectionProperties>().FirstOrDefault()
            ?? bodyEl.Descendants<SectionProperties>().FirstOrDefault();
        if (docSect != null)
        {
            var sp = BuildSection(docSect, main);
            if (sp.PageWidthPt.HasValue) ir.PageWidthPt = sp.PageWidthPt.Value;
            if (sp.PageHeightPt.HasValue) ir.PageHeightPt = sp.PageHeightPt.Value;
            if (sp.MarginLeftPt.HasValue) ir.MarginPt = sp.MarginLeftPt.Value;
        }

        foreach (var element in bodyEl.ChildElements)
        {
            switch (element)
            {
                case Paragraph p:
                    // 段落内的 sectPr 表示一个节边界
                    var inlineSect = p.ParagraphProperties?.SectionProperties;
                    AddParagraph(p, main, ir, ctx);
                    if (inlineSect != null)
                        ir.Blocks.Add(IrBlock.Of(BuildSection(inlineSect, main)));
                    break;
                case Table t:
                    ir.Blocks.Add(IrBlock.Of(BuildTable(t, main, ctx)));
                    break;
                case SectionProperties sp:
                    ir.Blocks.Add(IrBlock.Of(BuildSection(sp, main)));
                    break;
            }
        }

        return ir;
    }

    private static void AddParagraph(Paragraph p, MainDocumentPart main, IrDocument ir, WordContext ctx)
    {
        bool hasPageBreak = p.Descendants<Break>().Any(b => b.Type?.Value == BreakValues.Page);
        if (hasPageBreak)
        {
            ir.Blocks.Add(IrBlock.NewPage());
        }

        var images = ExtractImages(p, main).ToList();
        if (images.Count > 0)
        {
            foreach (var img in images)
                ir.Blocks.Add(IrBlock.Of(img));
            if (string.IsNullOrWhiteSpace(string.Concat(p.Descendants<Text>().Select(t => t.Text))))
                return;
        }

        // 解析样式继承得到的有效段落/run 默认属性
        var ip = BuildParagraph(p, ctx);
        // 若段落仅包含 OMML 公式（无普通 Run），BuildParagraph 会返回 null；此时仍构造一个空段以承载公式。
        if (ip == null)
        {
            var mathNsCheck = "http://schemas.openxmlformats.org/officeDocument/2006/math";
            bool hasOmml = p.Descendants().Any(e => e.NamespaceUri == mathNsCheck && (e.LocalName == "oMath" || e.LocalName == "oMathPara"));
            if (!hasOmml) return;
            ip = new IrParagraph();
        }

        // TOC 字段块级识别：扫描 SimpleField.Instruction 与 FieldChar 的 instr 文本
        if (ctx.EnableFields)
        {
            var tocInstr = DetectTocInstruction(p);
            if (tocInstr != null)
            {
                int maxLevel = ParseTocMaxLevel(tocInstr) ?? 3;
                ir.Blocks.Add(IrBlock.Of(new IrTocField { MaxLevel = maxLevel }));
                ctx.Diagnostics?.Info("WORD_TOC_FIELD", $"识别到 TOC 字段（max level={maxLevel}），将由渲染器生成目录。");
                // 若段落只是 TOC 占位（运行均被清空或仅为提示文本），跳过原段落输出
                if (ip.Runs.Count == 0) return;
            }
        }

        // OMML 公式：m:oMath / m:oMathPara → 通过 OmmlReader 线性化为可读文本，并标记 IsEquation
        var mathNs = "http://schemas.openxmlformats.org/officeDocument/2006/math";
        var mathRoots = p.Descendants()
            .Where(e => e.NamespaceUri == mathNs && (e.LocalName == "oMath" || e.LocalName == "oMathPara"))
            .ToList();
        if (mathRoots.Count > 0)
        {
            ip.IsEquation = true;
            var linearAll = new System.Text.StringBuilder();
            var mmlAll = new System.Text.StringBuilder();
            for (int mi = 0; mi < mathRoots.Count; mi++)
            {
                var linear = OmmlReader.Linearize(mathRoots[mi]);
                if (string.IsNullOrWhiteSpace(linear)) linear = "[公式]";
                ip.Runs.Add(new IrRun
                {
                    Text = (mi == 0 ? string.Empty : "  ") + linear,
                    IsMathPlaceholder = true,
                    Italic = true,
                });
                if (linearAll.Length > 0) linearAll.Append("  ");
                linearAll.Append(linear);
                var mml = OmmlReader.ToMathML(mathRoots[mi]);
                if (!string.IsNullOrEmpty(mml))
                {
                    if (mmlAll.Length > 0) mmlAll.Append('\n');
                    mmlAll.Append(mml);
                }
            }
            ip.EquationLinear = linearAll.ToString();
            if (mmlAll.Length > 0) ip.EquationMathML = mmlAll.ToString();
            ctx.Diagnostics?.Info("WORD_OMML_LINEARIZED", $"段落中包含 {mathRoots.Count} 个 OMML 公式，已线性化为文本。");
        }

        ir.Blocks.Add(IrBlock.Of(ip));

        // 文本框 / 形状内容：w:txbxContent → IrFloatingShape 占位块
        var txbxList = p.Descendants<DocumentFormat.OpenXml.Wordprocessing.TextBoxContent>().ToList();
        foreach (var tb in txbxList)
        {
            var shape = new IrFloatingShape();
            foreach (var inner in tb.Elements<Paragraph>())
            {
                var sip = BuildParagraph(inner, ctx);
                if (sip != null) shape.Paragraphs.Add(sip);
            }
            if (shape.Paragraphs.Count > 0)
            {
                ir.Blocks.Add(IrBlock.Of(shape));
                ctx.Diagnostics?.Info("WORD_TEXTBOX_FLATTENED", $"已将文本框/形状（{shape.Paragraphs.Count} 段）展平为浮动块。");
            }
        }
    }

    /// <summary>从 Word 段落构建 IR 段落（同时服务于正文与页眉页脚）。</summary>
    private static IrParagraph? BuildParagraph(Paragraph p, WordContext ctx)
    {
        var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        var resolved = ctx.ResolveParagraphStyle(styleId);

        bool isHeading = !string.IsNullOrEmpty(styleId)
            && (styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase)
                || styleId.StartsWith("标题", StringComparison.Ordinal));
        int level = 0;
        if (isHeading)
        {
            var levelStr = new string(styleId!.Where(char.IsDigit).ToArray());
            int.TryParse(levelStr, out level);
            if (level <= 0) level = 1;
        }

        // 列表元数据：渲染器据此画项目符号或编号，自动维护层级缩进
        var listInfo = ctx.ResolveListInfo(p);
        // 仍然消费 ResolveListPrefix 以推进内部计数器；prefix 字符串本身不再插入文本
        string? listPrefix = ctx.ResolveListPrefix(p);

        var pPr = p.ParagraphProperties;
        var ip = new IrParagraph
        {
            IsHeading = isHeading,
            HeadingLevel = level,
            Alignment = ParseAlignment(pPr?.Justification?.Val?.Value)
                        != HorizontalAlign.Left
                ? ParseAlignment(pPr?.Justification?.Val?.Value)
                : resolved.Alignment,
        };

        // 缩进（w:ind，单位为 1/20 pt）
        var ind = pPr?.Indentation;
        if (ind != null)
        {
            if (TryParseTwip(ind.FirstLine?.Value, out var fl)) ip.FirstLineIndentPt = fl;
            else if (TryParseTwip(ind.Hanging?.Value, out var hg)) ip.FirstLineIndentPt = -hg;
            if (TryParseTwip(ind.Left?.Value, out var lf)) ip.LeftIndentPt = lf;
            else if (TryParseTwip(ind.Start?.Value, out var st)) ip.LeftIndentPt = st;
        }

        // 间距（w:spacing）
        var sp = pPr?.SpacingBetweenLines;
        if (sp != null)
        {
            if (TryParseTwip(sp.Before?.Value, out var sb)) ip.SpaceBeforePt = sb;
            if (TryParseTwip(sp.After?.Value, out var sa)) ip.SpaceAfterPt = sa;
            // 行高：rule=auto 是 240 为单倍；exact/atLeast 是 twip
            if (sp.Line?.Value is { } lineVal && double.TryParse(lineVal, out var lineD))
            {
                var rule = sp.LineRule?.Value;
                if (rule == LineSpacingRuleValues.Exact || rule == LineSpacingRuleValues.AtLeast)
                    ip.LineHeightPt = lineD / 20.0;
                else
                    ip.LineHeightRatio = lineD / 240.0; // auto: 240 = 单倍行距
            }
        }
        // 缺失时回退到样式继承的默认值
        ip.SpaceBeforePt ??= resolved.SpaceBeforePt;
        ip.SpaceAfterPt ??= resolved.SpaceAfterPt;
        ip.LineHeightPt ??= resolved.LineHeightPt;
        ip.LineHeightRatio ??= resolved.LineHeightRatio;

        if (listInfo.HasValue)
        {
            ip.ListType = listInfo.Value.Type;
            ip.ListLevel = listInfo.Value.Level;
            if (!string.IsNullOrEmpty(listPrefix))
                ip.ListLabel = listPrefix.TrimEnd();
            // 解析当前编号（来自 ResolveListPrefix 已自增的计数器）：通过 prefix 抽取数字部分
            if (listInfo.Value.Type != ListType.Bullet && !string.IsNullOrEmpty(listPrefix))
            {
                var digits = new string(listPrefix.TakeWhile(ch => char.IsLetterOrDigit(ch)).ToArray());
                if (int.TryParse(digits, out var n)) ip.ListNumber = n;
            }
        }

        else if (!string.IsNullOrEmpty(listPrefix))
        {
            // 兜底：若仅有 prefix 没有结构化信息，仍以纯文本形式展示
            ip.Runs.Add(new IrRun
            {
                Text = listPrefix,
                FontFamily = resolved.RunDefaults.FontFamily,
                FontSize = resolved.RunDefaults.FontSize,
                Bold = resolved.RunDefaults.Bold,
                ColorHex = resolved.RunDefaults.ColorHex,
            });
        }

        // 段落边框 / 底纹
        var pBdr = pPr?.ParagraphBorders;
        if (pBdr != null)
        {
            ip.Border = new IrBorders
            {
                Top = ConvertBorder(pBdr.TopBorder),
                Right = ConvertBorder(pBdr.RightBorder),
                Bottom = ConvertBorder(pBdr.BottomBorder),
                Left = ConvertBorder(pBdr.LeftBorder),
            };
        }
        ip.BackgroundHex = NormalizeColor(pPr?.Shading?.Fill?.Value);

        // 制表位 w:tabs/w:tab
        var tabs = pPr?.Tabs;
        if (tabs != null)
        {
            foreach (var tab in tabs.Elements<TabStop>())
            {
                if (tab.Val?.Value == TabStopValues.Clear) continue;
                if (tab.Position?.Value is not int pos) continue;
                var ts = new IrTabStop { PositionPt = pos / 20.0 };
                var v = tab.Val?.Value;
                if (v == TabStopValues.Center) ts.Alignment = TabAlignment.Center;
                else if (v == TabStopValues.Right || v == TabStopValues.End) ts.Alignment = TabAlignment.Right;
                else if (v == TabStopValues.Decimal) ts.Alignment = TabAlignment.Decimal;
                else ts.Alignment = TabAlignment.Left;
                var ld = tab.Leader?.Value;
                if (ld == TabStopLeaderCharValues.Dot) ts.Leader = '.';
                else if (ld == TabStopLeaderCharValues.Underscore) ts.Leader = '_';
                else if (ld == TabStopLeaderCharValues.Hyphen) ts.Leader = '-';
                ip.TabStops.Add(ts);
            }
            if (ip.TabStops.Count > 1)
                ip.TabStops.Sort((a, b) => a.PositionPt.CompareTo(b.PositionPt));
        }

        // 需要跨 Run 近似跟踪 SimpleField / FieldChar （PAGE / NUMPAGES 域）
        // 简化：扫描所有子元素；SimpleField 按 instr 判别；
        // begin/end FieldChar 之间的 Run 按 instr 表达式判别，取中间结果 Run 作为占位。
        bool inField = false;
        string fieldInstr = string.Empty;
        bool sawSeparator = false;
        var fieldResultRuns = new List<int>();
        foreach (var child in p.ChildElements)
        {
            switch (child)
            {
                case SimpleField sf:
                    {
                        var instr = sf.Instruction?.Value ?? string.Empty;
                        var fieldKind = ClassifyField(instr);
                        if (fieldKind == PageField.Page || fieldKind == PageField.NumPages)
                        {
                            ip.Runs.Add(new IrRun
                            {
                                Text = "#",
                                IsPageNumberField = fieldKind == PageField.Page,
                                IsPageCountField = fieldKind == PageField.NumPages,
                                FieldKind = fieldKind == PageField.Page ? RunFieldKind.Page : RunFieldKind.NumPages,
                                FontFamily = resolved.RunDefaults.FontFamily,
                                FontSize = resolved.RunDefaults.FontSize,
                                ColorHex = resolved.RunDefaults.ColorHex,
                            });
                        }
                        else if (fieldKind == PageField.Section)
                        {
                            ip.Runs.Add(new IrRun
                            {
                                Text = "#",
                                FieldKind = RunFieldKind.Section,
                                FontFamily = resolved.RunDefaults.FontFamily,
                                FontSize = resolved.RunDefaults.FontSize,
                                ColorHex = resolved.RunDefaults.ColorHex,
                            });
                        }
                        else if (fieldKind == PageField.PageRef)
                        {
                            var bm = ExtractFieldArgument(instr, "PAGEREF");
                            ip.Runs.Add(new IrRun
                            {
                                Text = "#",
                                PageRefAnchor = string.IsNullOrEmpty(bm) ? null : MakeBookmarkAnchor(bm!),
                                FontFamily = resolved.RunDefaults.FontFamily,
                                FontSize = resolved.RunDefaults.FontSize,
                                ColorHex = resolved.RunDefaults.ColorHex,
                            });
                        }
                        else if (fieldKind == PageField.Date || fieldKind == PageField.Time)
                        {
                            ip.Runs.Add(new IrRun
                            {
                                Text = fieldKind == PageField.Date
                                    ? DateTime.Now.ToString("yyyy-MM-dd")
                                    : DateTime.Now.ToString("HH:mm"),
                                FontFamily = resolved.RunDefaults.FontFamily,
                                FontSize = resolved.RunDefaults.FontSize,
                                ColorHex = resolved.RunDefaults.ColorHex,
                            });
                        }
                        else
                        {
                            // 取 SimpleField 内部的表象文本
                            foreach (var run in sf.Elements<Run>())
                                AppendRun(ip, run, ctx, resolved);
                        }
                        break;
                    }
                case Run r:
                    {
                        // 处理复杂域：begin/separate/end
                        var fc = r.Elements<FieldChar>().FirstOrDefault()?.FieldCharType?.Value;
                        if (fc == FieldCharValues.Begin)
                        {
                            inField = true;
                            fieldInstr = string.Empty;
                            sawSeparator = false;
                            fieldResultRuns.Clear();
                            break;
                        }
                        if (fc == FieldCharValues.Separate)
                        {
                            sawSeparator = true;
                            break;
                        }
                        if (fc == FieldCharValues.End)
                        {
                            var kind = ClassifyField(fieldInstr);
                            if (kind == PageField.Page || kind == PageField.NumPages)
                            {
                                // 替换结果 Run 为页码占位
                                foreach (var idx in fieldResultRuns)
                                {
                                    ip.Runs[idx].Text = "#";
                                    ip.Runs[idx].IsPageNumberField = kind == PageField.Page;
                                    ip.Runs[idx].IsPageCountField = kind == PageField.NumPages;
                                    ip.Runs[idx].FieldKind = kind == PageField.Page ? RunFieldKind.Page : RunFieldKind.NumPages;
                                }
                                if (fieldResultRuns.Count == 0)
                                {
                                    ip.Runs.Add(new IrRun
                                    {
                                        Text = "#",
                                        IsPageNumberField = kind == PageField.Page,
                                        IsPageCountField = kind == PageField.NumPages,
                                        FieldKind = kind == PageField.Page ? RunFieldKind.Page : RunFieldKind.NumPages,
                                        FontFamily = resolved.RunDefaults.FontFamily,
                                        FontSize = resolved.RunDefaults.FontSize,
                                        ColorHex = resolved.RunDefaults.ColorHex,
                                    });
                                }
                            }
                            else if (kind == PageField.Section)
                            {
                                foreach (var idx in fieldResultRuns)
                                {
                                    ip.Runs[idx].Text = "#";
                                    ip.Runs[idx].FieldKind = RunFieldKind.Section;
                                }
                                if (fieldResultRuns.Count == 0)
                                {
                                    ip.Runs.Add(new IrRun
                                    {
                                        Text = "#",
                                        FieldKind = RunFieldKind.Section,
                                        FontFamily = resolved.RunDefaults.FontFamily,
                                        FontSize = resolved.RunDefaults.FontSize,
                                        ColorHex = resolved.RunDefaults.ColorHex,
                                    });
                                }
                            }
                            else if (kind == PageField.PageRef)
                            {
                                var bm = ExtractFieldArgument(fieldInstr, "PAGEREF");
                                var anchor = string.IsNullOrEmpty(bm) ? null : MakeBookmarkAnchor(bm!);
                                if (fieldResultRuns.Count > 0)
                                {
                                    foreach (var idx in fieldResultRuns)
                                        ip.Runs[idx].PageRefAnchor = anchor;
                                }
                                else
                                {
                                    ip.Runs.Add(new IrRun
                                    {
                                        Text = "#",
                                        PageRefAnchor = anchor,
                                        FontFamily = resolved.RunDefaults.FontFamily,
                                        FontSize = resolved.RunDefaults.FontSize,
                                        ColorHex = resolved.RunDefaults.ColorHex,
                                    });
                                }
                            }
                            else if (kind == PageField.Ref)
                            {
                                // REF 字段：保留结果文本不动；若为空则忽略
                            }
                            else if (kind == PageField.Hyperlink)
                            {
                                var url = ExtractFieldArgument(fieldInstr, "HYPERLINK");
                                if (!string.IsNullOrEmpty(url))
                                {
                                    foreach (var idx in fieldResultRuns)
                                    {
                                        ip.Runs[idx].HyperlinkUrl = url;
                                        if (string.IsNullOrEmpty(ip.Runs[idx].ColorHex))
                                            ip.Runs[idx].ColorHex = "#0563C1";
                                        ip.Runs[idx].Underline = true;
                                    }
                                }
                            }
                            else if (kind == PageField.Date || kind == PageField.Time)
                            {
                                var now = kind == PageField.Date
                                    ? DateTime.Now.ToString("yyyy-MM-dd")
                                    : DateTime.Now.ToString("HH:mm");
                                if (fieldResultRuns.Count > 0)
                                {
                                    foreach (var idx in fieldResultRuns) ip.Runs[idx].Text = now;
                                }
                                else
                                {
                                    ip.Runs.Add(new IrRun
                                    {
                                        Text = now,
                                        FontFamily = resolved.RunDefaults.FontFamily,
                                        FontSize = resolved.RunDefaults.FontSize,
                                        ColorHex = resolved.RunDefaults.ColorHex,
                                    });
                                }
                            }
                            else if (kind == PageField.Toc)
                            {
                                // 移除已收集的目录占位结果 Run，由渲染器在段落级生成 TOC
                                if (fieldResultRuns.Count > 0)
                                {
                                    fieldResultRuns.Sort();
                                    for (int j = fieldResultRuns.Count - 1; j >= 0; j--)
                                        ip.Runs.RemoveAt(fieldResultRuns[j]);
                                }
                            }
                            inField = false;
                            fieldInstr = string.Empty;
                            sawSeparator = false;
                            fieldResultRuns.Clear();
                            break;
                        }

                        if (inField)
                        {
                            // 在 instr 阶段收集指令文本
                            if (!sawSeparator)
                            {
                                fieldInstr += string.Concat(r.Elements<FieldCode>().Select(c => c.Text))
                                              + string.Concat(r.Elements<Text>().Select(t => t.Text));
                            }
                            else
                            {
                                // 结果阶段：保留结果文本，记录索引以便事后改写
                                int before = ip.Runs.Count;
                                AppendRun(ip, r, ctx, resolved);
                                for (int i = before; i < ip.Runs.Count; i++)
                                    fieldResultRuns.Add(i);
                            }
                            break;
                        }

                        AppendRun(ip, r, ctx, resolved);
                        break;
                    }
                case DocumentFormat.OpenXml.Wordprocessing.Hyperlink hp:
                    {
                        var url = ctx.ResolveHyperlink(hp.Id?.Value);
                        // 内部书签跳转：w:anchor 指向某个 w:bookmarkStart 名称
                        var anchor = hp.Anchor?.Value;
                        foreach (var run in hp.Elements<Run>())
                        {
                            int before = ip.Runs.Count;
                            AppendRun(ip, run, ctx, resolved);
                            for (int i = before; i < ip.Runs.Count; i++)
                            {
                                if (!string.IsNullOrEmpty(url))
                                    ip.Runs[i].HyperlinkUrl = url;
                                if (!string.IsNullOrEmpty(anchor))
                                    ip.Runs[i].AnchorRef = MakeBookmarkAnchor(anchor!);
                                if (!string.IsNullOrEmpty(url) || !string.IsNullOrEmpty(anchor))
                                {
                                    if (string.IsNullOrEmpty(ip.Runs[i].ColorHex))
                                        ip.Runs[i].ColorHex = "#0563C1";
                                    ip.Runs[i].Underline = true;
                                }
                            }
                        }
                        break;
                    }
                case InsertedRun ins:
                    {
                        foreach (var run in ins.Elements<Run>())
                        {
                            int before = ip.Runs.Count;
                            AppendRun(ip, run, ctx, resolved);
                            for (int i = before; i < ip.Runs.Count; i++)
                            {
                                ip.Runs[i].IsInsertion = true;
                                ip.Runs[i].Underline = true;
                                if (string.IsNullOrEmpty(ip.Runs[i].ColorHex))
                                    ip.Runs[i].ColorHex = "#1F7A1F";
                            }
                        }
                        break;
                    }
                case DeletedRun del:
                    {
                        foreach (var run in del.Elements<Run>())
                        {
                            int before = ip.Runs.Count;
                            // DeletedRun 中文本保存在 w:delText
                            var delText = string.Concat(run.Descendants<DeletedText>().Select(t => t.Text));
                            if (!string.IsNullOrEmpty(delText))
                            {
                                var rp = run.RunProperties;
                                ip.Runs.Add(new IrRun
                                {
                                    Text = delText,
                                    FontFamily = rp?.RunFonts?.Ascii?.Value ?? resolved.RunDefaults.FontFamily,
                                    FontSize = ParseHalfPoint(rp?.FontSize?.Val?.Value) ?? resolved.RunDefaults.FontSize,
                                    Bold = rp?.Bold != null || resolved.RunDefaults.Bold,
                                    Italic = rp?.Italic != null || resolved.RunDefaults.Italic,
                                    ColorHex = "#B22222",
                                    IsDeletion = true,
                                });
                            }
                            for (int i = before; i < ip.Runs.Count; i++)
                                ip.Runs[i].IsDeletion = true;
                        }
                        break;
                    }
                case CommentRangeStart crs:
                    {
                        if (crs.Id?.Value is { } cid)
                        {
                            ip.Runs.Add(new IrRun
                            {
                                Text = string.Empty,
                                CommentRef = cid.ToString(),
                            });
                        }
                        break;
                    }
            }
        }

        if (ip.Runs.Count == 0 && !isHeading) return null;

        // 为标题段落生成稳定 anchor id，供 PDF 大纲（书签）与内部跳转使用。
        if (isHeading)
        {
            ip.AnchorId = MakeHeadingAnchor(level, ip.PlainText);
        }
        else
        {
            // 非标题段落：如果包含 w:bookmarkStart，则用其名称构造 anchor id，便于 PAGEREF/Hyperlink 内部跳转。
            var bm = p.Descendants<BookmarkStart>()
                .FirstOrDefault(b => !string.IsNullOrEmpty(b.Name?.Value)
                    && !b.Name!.Value!.StartsWith("_GoBack", StringComparison.OrdinalIgnoreCase));
            if (bm != null)
            {
                ip.AnchorId = MakeBookmarkAnchor(bm.Name!.Value!);
            }
        }

        // 把段落上所有自定义书签名收集到 BookmarkNames，便于 PDF/XPS 命名锚点。
        foreach (var bms in p.Descendants<BookmarkStart>())
        {
            var name = bms.Name?.Value;
            if (string.IsNullOrEmpty(name)) continue;
            if (name!.StartsWith("_GoBack", StringComparison.OrdinalIgnoreCase)) continue;
            var anchor = MakeBookmarkAnchor(name);
            if (!ip.BookmarkNames.Contains(anchor)) ip.BookmarkNames.Add(anchor);
        }
        return ip;
    }

    private static string MakeBookmarkAnchor(string name)
    {
        var safe = string.Concat((name ?? string.Empty).Where(c => char.IsLetterOrDigit(c) || c == '_'));
        if (safe.Length > 48) safe = safe.Substring(0, 48);
        if (string.IsNullOrEmpty(safe)) safe = "anon";
        return "bm_" + safe;
    }

    private static int s_headingCounter;
    private static string MakeHeadingAnchor(int level, string text)
    {
        var n = System.Threading.Interlocked.Increment(ref s_headingCounter);
        var safe = string.Concat((text ?? string.Empty).Where(c => char.IsLetterOrDigit(c)));
        if (safe.Length > 24) safe = safe.Substring(0, 24);
        return $"h{level}_{n:D4}_{safe}";
    }

    private enum PageField { None, Page, NumPages, PageRef, Hyperlink, Date, Time, Ref, Toc, Section }

    private static PageField ClassifyField(string instr)
    {
        if (string.IsNullOrWhiteSpace(instr)) return PageField.None;
        var t = instr.TrimStart();
        if (t.StartsWith("PAGEREF", StringComparison.OrdinalIgnoreCase)) return PageField.PageRef;
        if (t.StartsWith("HYPERLINK", StringComparison.OrdinalIgnoreCase)) return PageField.Hyperlink;
        if (t.StartsWith("NUMPAGES", StringComparison.OrdinalIgnoreCase) || t.StartsWith("PAGES", StringComparison.OrdinalIgnoreCase))
            return PageField.NumPages;
        if (t.StartsWith("PAGE", StringComparison.OrdinalIgnoreCase)) return PageField.Page;
        if (t.StartsWith("SECTION", StringComparison.OrdinalIgnoreCase)) return PageField.Section;
        if (t.StartsWith("DATE", StringComparison.OrdinalIgnoreCase) || t.StartsWith("CREATEDATE", StringComparison.OrdinalIgnoreCase) || t.StartsWith("SAVEDATE", StringComparison.OrdinalIgnoreCase) || t.StartsWith("PRINTDATE", StringComparison.OrdinalIgnoreCase))
            return PageField.Date;
        if (t.StartsWith("TIME", StringComparison.OrdinalIgnoreCase)) return PageField.Time;
        if (t.StartsWith("REF", StringComparison.OrdinalIgnoreCase)) return PageField.Ref;
        if (t.StartsWith("TOC", StringComparison.OrdinalIgnoreCase)) return PageField.Toc;
        return PageField.None;
    }

    /// <summary>从 PAGEREF/REF 指令中抽取目标书签名（首个非选项 token）。</summary>
    private static string? ExtractFieldArgument(string instr, string keyword)
    {
        if (string.IsNullOrEmpty(instr)) return null;
        var s = instr.Trim();
        var idx = s.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        s = s.Substring(idx + keyword.Length).TrimStart();
        // 第一个 token：支持引号
        if (s.Length == 0) return null;
        if (s[0] == '"')
        {
            var end = s.IndexOf('"', 1);
            if (end < 0) return null;
            return s.Substring(1, end - 1);
        }
        var sp = s.IndexOfAny(new[] { ' ', '\t', '\\' });
        return sp < 0 ? s : s.Substring(0, sp);
    }

    /// <summary>扫描段落中所有字段指令，返回首个 TOC 指令文本（或 null）。</summary>
    private static string? DetectTocInstruction(Paragraph p)
    {
        foreach (var sf in p.Descendants<SimpleField>())
        {
            var instr = sf.Instruction?.Value;
            if (!string.IsNullOrWhiteSpace(instr) && instr!.TrimStart().StartsWith("TOC", StringComparison.OrdinalIgnoreCase))
                return instr;
        }
        // 复杂域：begin..separate 之间的 instr 文本
        var sb = new System.Text.StringBuilder();
        bool inField = false; bool sawSep = false; string current = string.Empty;
        foreach (var r in p.Descendants<Run>())
        {
            var fc = r.Elements<FieldChar>().FirstOrDefault()?.FieldCharType?.Value;
            if (fc == FieldCharValues.Begin) { inField = true; sawSep = false; current = string.Empty; continue; }
            if (fc == FieldCharValues.Separate) { sawSep = true; continue; }
            if (fc == FieldCharValues.End)
            {
                if (current.TrimStart().StartsWith("TOC", StringComparison.OrdinalIgnoreCase)) return current;
                inField = false; sawSep = false; current = string.Empty; continue;
            }
            if (inField && !sawSep)
            {
                current += string.Concat(r.Elements<FieldCode>().Select(c => c.Text))
                         + string.Concat(r.Elements<Text>().Select(t => t.Text));
            }
        }
        return null;
    }

    /// <summary>解析 TOC 指令的 \o "1-3" 选项；缺省返回 null。</summary>
    private static int? ParseTocMaxLevel(string instr)
    {
        if (string.IsNullOrEmpty(instr)) return null;
        var idx = instr.IndexOf("\\o", StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return null;
        var rest = instr.Substring(idx + 2).TrimStart();
        if (rest.Length == 0 || rest[0] != '"') return null;
        var end = rest.IndexOf('"', 1);
        if (end < 0) return null;
        var range = rest.Substring(1, end - 1);
        var dash = range.IndexOf('-');
        var tail = dash >= 0 ? range.Substring(dash + 1) : range;
        return int.TryParse(tail, out var n) ? n : (int?)null;
    }

    private static void AppendRun(IrParagraph ip, Run run, WordContext ctx, ResolvedStyle resolved)
    {
        // 批注引用
        var cmtRef = run.Elements<CommentReference>().FirstOrDefault();
        if (cmtRef?.Id?.Value is { } cmtid)
        {
            ip.Runs.Add(new IrRun
            {
                Text = "*",
                CommentRef = cmtid.ToString(),
                FontFamily = resolved.RunDefaults.FontFamily,
                FontSize = resolved.RunDefaults.FontSize,
                ColorHex = "#A0522D",
            });
            return;
        }
        // 脚注/尾注引用：上标编号 + 关联到 IrFootnote
        var fnRef = run.Elements<FootnoteReference>().FirstOrDefault();
        if (fnRef?.Id?.Value is { } fnid)
        {
            ip.Runs.Add(new IrRun
            {
                Text = "*",
                FootnoteRef = fnid.ToString(),
                FontFamily = resolved.RunDefaults.FontFamily,
                FontSize = resolved.RunDefaults.FontSize,
                ColorHex = resolved.RunDefaults.ColorHex,
            });
            return;
        }
        var enRef = run.Elements<EndnoteReference>().FirstOrDefault();
        if (enRef?.Id?.Value is { } enid)
        {
            ip.Runs.Add(new IrRun
            {
                Text = "*",
                EndnoteRef = enid.ToString(),
                FontFamily = resolved.RunDefaults.FontFamily,
                FontSize = resolved.RunDefaults.FontSize,
                ColorHex = resolved.RunDefaults.ColorHex,
            });
            return;
        }
        var text = string.Concat(run.Elements<Text>().Select(t => t.Text));
        if (string.IsNullOrEmpty(text)) return;
        var rp = run.RunProperties;
        var runStyleId = rp?.RunStyle?.Val?.Value;
        var runResolved = ctx.ResolveRunStyle(runStyleId, resolved.RunDefaults);

        ip.Runs.Add(new IrRun
        {
            Text = text,
            FontFamily = rp?.RunFonts?.Ascii?.Value
                         ?? rp?.RunFonts?.EastAsia?.Value
                         ?? runResolved.FontFamily,
            FontSize = ParseHalfPoint(rp?.FontSize?.Val?.Value) ?? runResolved.FontSize,
            Bold = rp?.Bold != null || runResolved.Bold,
            Italic = rp?.Italic != null || runResolved.Italic,
            Underline = rp?.Underline != null || runResolved.Underline,
            ColorHex = NormalizeColor(rp?.Color?.Val?.Value) ?? runResolved.ColorHex,
            HighlightHex = ResolveHighlight(rp),
        });
    }

    private static string? ResolveHighlight(RunProperties? rp)
    {
        if (rp == null) return null;
        var hl = rp.Highlight?.Val?.Value;
        if (hl != null && hl != HighlightColorValues.None)
        {
            return hl.Value switch
            {
                _ when hl == HighlightColorValues.Yellow => "#FFFF00",
                _ when hl == HighlightColorValues.Green => "#00FF00",
                _ when hl == HighlightColorValues.Cyan => "#00FFFF",
                _ when hl == HighlightColorValues.Magenta => "#FF00FF",
                _ when hl == HighlightColorValues.Blue => "#0000FF",
                _ when hl == HighlightColorValues.Red => "#FF0000",
                _ when hl == HighlightColorValues.DarkBlue => "#00008B",
                _ when hl == HighlightColorValues.DarkCyan => "#008B8B",
                _ when hl == HighlightColorValues.DarkGreen => "#006400",
                _ when hl == HighlightColorValues.DarkMagenta => "#8B008B",
                _ when hl == HighlightColorValues.DarkRed => "#8B0000",
                _ when hl == HighlightColorValues.DarkYellow => "#808000",
                _ when hl == HighlightColorValues.DarkGray => "#A9A9A9",
                _ when hl == HighlightColorValues.LightGray => "#D3D3D3",
                _ when hl == HighlightColorValues.Black => "#000000",
                _ when hl == HighlightColorValues.White => "#FFFFFF",
                _ => null,
            };
        }
        var shdFill = rp.Shading?.Fill?.Value;
        return NormalizeColor(shdFill);
    }

    private static bool TryParseTwip(string? twip, out double pt)
    {
        if (double.TryParse(twip, out var v)) { pt = v / 20.0; return true; }
        pt = 0; return false;
    }

    private static IrBorder? ConvertBorder(BorderType? b)
    {
        if (b == null) return null;
        var val = b.Val?.Value;
        if (val == null || val == BorderValues.Nil || val == BorderValues.None) return null;
        // size 单位为 1/8 pt
        double thickness = 0.5;
        if (b.Size?.Value is { } sz && sz > 0) thickness = sz / 8.0;
        var color = NormalizeColor(b.Color?.Value);
        return new IrBorder { Thickness = thickness, ColorHex = color ?? "#000000" };
    }

    private static IrSectionProperties BuildSection(SectionProperties sp, MainDocumentPart main)
    {
        var s = new IrSectionProperties();
        var size = sp.Elements<PageSize>().FirstOrDefault();
        if (size?.Width?.Value != null) s.PageWidthPt = size.Width.Value / 20.0;
        if (size?.Height?.Value != null) s.PageHeightPt = size.Height.Value / 20.0;
        if (size?.Orient?.Value == PageOrientationValues.Landscape) s.Orientation = PageOrientation.Landscape;

        var margin = sp.Elements<PageMargin>().FirstOrDefault();
        if (margin?.Top?.Value != null) s.MarginTopPt = margin.Top.Value / 20.0;
        if (margin?.Bottom?.Value != null) s.MarginBottomPt = margin.Bottom.Value / 20.0;
        if (margin?.Left?.Value != null) s.MarginLeftPt = margin.Left.Value / 20.0;
        if (margin?.Right?.Value != null) s.MarginRightPt = margin.Right.Value / 20.0;

        var typeVal = sp.Elements<SectionType>().FirstOrDefault()?.Val?.Value;
        if (typeVal == SectionMarkValues.NextPage || typeVal == SectionMarkValues.OddPage || typeVal == SectionMarkValues.EvenPage)
            s.BreakBefore = true;

        // 从 HeaderReference / FooterReference 解析真实页眉页脚（取第一个非首页/非偶页的 default）
        var headerRef = sp.Elements<HeaderReference>()
            .FirstOrDefault(r => r.Type?.Value != HeaderFooterValues.First && r.Type?.Value != HeaderFooterValues.Even)
            ?? sp.Elements<HeaderReference>().FirstOrDefault();
        if (headerRef?.Id?.Value is { } hrid && main.GetPartById(hrid) is HeaderPart hp)
        {
            ExtractHeaderFooterParagraphs(hp.Header, s.HeaderParagraphs);
            s.HeaderText = string.Join(" ", s.HeaderParagraphs.Select(x => x.PlainText)).Trim();
            if (string.IsNullOrEmpty(s.HeaderText)) s.HeaderText = null;
        }
        var footerRef = sp.Elements<FooterReference>()
            .FirstOrDefault(r => r.Type?.Value != HeaderFooterValues.First && r.Type?.Value != HeaderFooterValues.Even)
            ?? sp.Elements<FooterReference>().FirstOrDefault();
        if (footerRef?.Id?.Value is { } frid && main.GetPartById(frid) is FooterPart fp)
        {
            ExtractHeaderFooterParagraphs(fp.Footer, s.FooterParagraphs);
            s.FooterText = string.Join(" ", s.FooterParagraphs.Select(x => x.PlainText)).Trim();
            if (string.IsNullOrEmpty(s.FooterText)) s.FooterText = null;
        }

        // 偶数页页眉/页脚
        var headerEvenRef = sp.Elements<HeaderReference>().FirstOrDefault(r => r.Type?.Value == HeaderFooterValues.Even);
        if (headerEvenRef?.Id?.Value is { } herid && main.GetPartById(herid) is HeaderPart hep)
        {
            ExtractHeaderFooterParagraphs(hep.Header, s.HeaderEvenParagraphs);
            s.DifferentOddEven = true;
        }
        var footerEvenRef = sp.Elements<FooterReference>().FirstOrDefault(r => r.Type?.Value == HeaderFooterValues.Even);
        if (footerEvenRef?.Id?.Value is { } ferid && main.GetPartById(ferid) is FooterPart fep)
        {
            ExtractHeaderFooterParagraphs(fep.Footer, s.FooterEvenParagraphs);
            s.DifferentOddEven = true;
        }
        // 首页页眉/页脚
        var headerFirstRef = sp.Elements<HeaderReference>().FirstOrDefault(r => r.Type?.Value == HeaderFooterValues.First);
        if (headerFirstRef?.Id?.Value is { } hfrid && main.GetPartById(hfrid) is HeaderPart hfp)
        {
            ExtractHeaderFooterParagraphs(hfp.Header, s.HeaderFirstParagraphs);
            s.DifferentFirstPage = true;
        }
        var footerFirstRef = sp.Elements<FooterReference>().FirstOrDefault(r => r.Type?.Value == HeaderFooterValues.First);
        if (footerFirstRef?.Id?.Value is { } ffrid && main.GetPartById(ffrid) is FooterPart ffp)
        {
            ExtractHeaderFooterParagraphs(ffp.Footer, s.FooterFirstParagraphs);
            s.DifferentFirstPage = true;
        }
        // 节级 titlePg 标记（首页不同）
        if (sp.Elements<TitlePage>().Any()) s.DifferentFirstPage = true;

        // 分栏 w:cols
        var cols = sp.Elements<Columns>().FirstOrDefault();
        if (cols != null)
        {
            if (cols.ColumnCount?.Value is { } cn && cn > 1) s.ColumnCount = cn;
            if (cols.Space?.Value is { } sp2 && double.TryParse(sp2, out var spt))
                s.ColumnSpacingPt = spt / 20.0;
        }

        // 页码格式 w:pgNumType
        var pgnt = sp.Elements<PageNumberType>().FirstOrDefault();
        if (pgnt != null)
        {
            if (pgnt.Format?.Value is { } fmt)
            {
                if (fmt == NumberFormatValues.LowerLetter) s.PageNumberFormat = PageNumberFormat.LowerLetter;
                else if (fmt == NumberFormatValues.UpperLetter) s.PageNumberFormat = PageNumberFormat.UpperLetter;
                else if (fmt == NumberFormatValues.LowerRoman) s.PageNumberFormat = PageNumberFormat.LowerRoman;
                else if (fmt == NumberFormatValues.UpperRoman) s.PageNumberFormat = PageNumberFormat.UpperRoman;
                else s.PageNumberFormat = PageNumberFormat.Decimal;
            }
            if (pgnt.Start?.Value is { } st) s.PageNumberStart = st;
        }

        return s;
    }

    private static void ExtractHeaderFooterParagraphs(DocumentFormat.OpenXml.OpenXmlElement? root, List<IrParagraph> sink)
    {
        if (root == null) return;
        var ctx = TryGetContextFromAncestor(root);
        foreach (var para in root.Descendants<Paragraph>())
        {
            var ip = ctx != null ? BuildParagraph(para, ctx) : BuildSimpleParagraph(para);
            if (ip != null && (ip.Runs.Count > 0)) sink.Add(ip);
        }
    }

    private static IrParagraph BuildSimpleParagraph(Paragraph p)
    {
        var ip = new IrParagraph
        {
            Alignment = ParseAlignment(p.ParagraphProperties?.Justification?.Val?.Value),
        };
        var text = string.Concat(p.Descendants<Text>().Select(t => t.Text));
        if (!string.IsNullOrEmpty(text)) ip.Runs.Add(new IrRun { Text = text });
        return ip;
    }

    /// <summary>把 footnotes/endnotes 部件的子项导入字典；过滤 separator/continuationSeparator/continuationNotice。</summary>
    private static void LoadNotes(DocumentFormat.OpenXml.OpenXmlElement? notesRoot, Dictionary<string, IrFootnote> sink)
    {
        if (notesRoot == null) return;
        int n = 0;
        foreach (var child in notesRoot.Elements())
        {
            string? id = null;
            string? typeStr = null;
            switch (child)
            {
                case Footnote fn:
                    id = fn.Id?.Value.ToString();
                    typeStr = fn.Type?.Value.ToString();
                    break;
                case Endnote en:
                    id = en.Id?.Value.ToString();
                    typeStr = en.Type?.Value.ToString();
                    break;
                default:
                    continue;
            }
            if (string.IsNullOrEmpty(id)) continue;
            // 过滤系统注释
            if (!string.IsNullOrEmpty(typeStr) &&
                (typeStr!.Equals("separator", StringComparison.OrdinalIgnoreCase)
                 || typeStr.Equals("continuationSeparator", StringComparison.OrdinalIgnoreCase)
                 || typeStr.Equals("continuationNotice", StringComparison.OrdinalIgnoreCase)))
                continue;
            var note = new IrFootnote { Id = id!, Number = ++n };
            foreach (var para in child.Descendants<Paragraph>())
            {
                var ip = BuildSimpleParagraph(para);
                if (ip.Runs.Count > 0) note.Paragraphs.Add(ip);
            }
            sink[id!] = note;
        }
    }

    private static void LoadComments(Comments? commentsRoot, Dictionary<string, IrComment> sink)
    {
        if (commentsRoot == null) return;
        int n = 0;
        foreach (var c in commentsRoot.Elements<Comment>())
        {
            var id = c.Id?.Value.ToString();
            if (string.IsNullOrEmpty(id)) continue;
            var ic = new IrComment
            {
                Id = id!,
                Number = ++n,
                Author = c.Author?.Value,
                Initials = c.Initials?.Value,
            };
            if (c.Date?.Value is DateTime dt) ic.Date = dt;
            foreach (var para in c.Descendants<Paragraph>())
            {
                var ip = BuildSimpleParagraph(para);
                if (ip.Runs.Count > 0) ic.Paragraphs.Add(ip);
            }
            sink[id!] = ic;
        }
    }

    private static WordContext? TryGetContextFromAncestor(DocumentFormat.OpenXml.OpenXmlElement root)
    {
        // HeaderPart/FooterPart 不是 Wordprocessing 树中的节点，这里仅用于未来扩展；
        // 目前总是返回 null 以采用简化路径，避免对页眉页脚中样式表的错误依赖。
        return null;
    }

    private static IEnumerable<IrImage> ExtractImages(Paragraph p, MainDocumentPart main)
    {
        // 普通嵌入图片：a:blip 引用 ImagePart
        foreach (var blip in p.Descendants<A.Blip>())
        {
            var rid = blip.Embed?.Value;
            if (string.IsNullOrEmpty(rid)) continue;
            if (main.GetPartById(rid) is ImagePart imagePart)
            {
                using var s = imagePart.GetStream();
                using var mem = new MemoryStream();
                s.CopyTo(mem);
                // 浮动模式判断：祖先若为 wp:anchor 则按对齐推断 Float；否则 Inline。
                var floatMode = ImageFloatMode.Inline;
                var anchor = blip.Ancestors<DocumentFormat.OpenXml.Drawing.Wordprocessing.Anchor>().FirstOrDefault();
                if (anchor != null)
                {
                    floatMode = ImageFloatMode.Center;
                    var hPos = anchor.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.HorizontalPosition>().FirstOrDefault();
                    var hAlign = hPos?.Descendants<DocumentFormat.OpenXml.Drawing.Wordprocessing.HorizontalAlignment>().FirstOrDefault()?.InnerText;
                    if (string.Equals(hAlign, "left", StringComparison.OrdinalIgnoreCase)) floatMode = ImageFloatMode.Left;
                    else if (string.Equals(hAlign, "right", StringComparison.OrdinalIgnoreCase)) floatMode = ImageFloatMode.Right;
                    else if (string.Equals(hAlign, "center", StringComparison.OrdinalIgnoreCase)) floatMode = ImageFloatMode.Center;
                }

                // 尺寸：从 wp:extent 读取（EMU，1 EMU = 1/914400 in，1 in = 96 px）
                double? widthPx = null, heightPx = null;
                var extent = blip.Ancestors<DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline>()
                    .FirstOrDefault()?.Extent
                    ?? blip.Ancestors<DocumentFormat.OpenXml.Drawing.Wordprocessing.Anchor>()
                    .FirstOrDefault()?.Extent;
                if (extent != null)
                {
                    if (extent.Cx != null) widthPx = extent.Cx.Value / 914400.0 * 96.0;
                    if (extent.Cy != null) heightPx = extent.Cy.Value / 914400.0 * 96.0;
                }

                var ctype = imagePart.ContentType?.Replace("image/", string.Empty);
                var rawBytes = mem.ToArray();
                bool isVector = IsVectorContentType(ctype);
                byte[] dataBytes = rawBytes;
                string? vectorFormat = null;
                if (isVector)
                {
                    vectorFormat = ctype;
                    dataBytes = EmfWmfHelper.RasterizeToPng(rawBytes, ctype);
                }
                yield return new IrImage
                {
                    Data = dataBytes,
                    Format = isVector ? "png" : ctype,
                    Float = floatMode,
                    WidthPx = widthPx,
                    HeightPx = heightPx,
                    IsVector = isVector,
                    VectorFormat = vectorFormat,
                };
            }
        }

        // 图表：使用图表部件中的 PNG/EMF 缓存（若存在）
        foreach (var chartRef in p.Descendants<DocumentFormat.OpenXml.Drawing.Charts.ChartReference>())
        {
            var rid = chartRef.Id?.Value;
            if (string.IsNullOrEmpty(rid)) continue;
            var chartPart = main.GetPartById(rid) as ChartPart;
            if (chartPart == null) continue;
            // 尝试取 chartPart 内部的 ImagePart 缓存
            foreach (var ip in chartPart.ImageParts)
            {
                using var s = ip.GetStream();
                using var mem = new MemoryStream();
                s.CopyTo(mem);
                var ctype = ip.ContentType?.Replace("image/", string.Empty);
                var rawBytes = mem.ToArray();
                bool isVector = IsVectorContentType(ctype);
                byte[] dataBytes = rawBytes;
                string? vectorFormat = null;
                if (isVector)
                {
                    vectorFormat = ctype;
                    dataBytes = EmfWmfHelper.RasterizeToPng(rawBytes, ctype);
                }
                yield return new IrImage
                {
                    Data = dataBytes,
                    Format = isVector ? "png" : ctype,
                    IsVector = isVector,
                    VectorFormat = vectorFormat,
                };
            }
        }
    }

    private static bool IsVectorContentType(string? ctype)
    {
        if (string.IsNullOrEmpty(ctype)) return false;
        var c = ctype!.ToLowerInvariant();
        return c.Contains("emf") || c.Contains("wmf") || c.Contains("x-emf") || c.Contains("x-wmf") || c.Contains("svg");
    }

    private static IrTable BuildTable(Table t, MainDocumentPart main, WordContext ctx)
    {
        _ = ctx;
        var table = new IrTable();
        var grid = t.Elements<TableGrid>().FirstOrDefault();
        if (grid != null)
        {
            foreach (var col in grid.Elements<GridColumn>())
            {
                if (col.Width?.Value != null && double.TryParse(col.Width.Value, out var w))
                {
                    table.ColumnWidthsPt.Add(w / 20.0);
                }
                else
                {
                    table.ColumnWidthsPt.Add(null);
                }
            }
        }

        var rows = t.Elements<TableRow>().ToList();
        // 第一遍：构造行/单元格，并记录 vMerge
        var matrix = new List<List<IrCell>>();
        // 记录每行是否声明为表头行（<w:tblHeader/>）
        var rowIsHeader = new List<bool>(rows.Count);
        foreach (var tr in rows)
        {
            // 行属性中存在 <w:tblHeader/> 即视为重复表头行；空 Val 默认为 true。
            var trPr = tr.Elements<TableRowProperties>().FirstOrDefault();
            bool isHeader = false;
            if (trPr != null)
            {
                foreach (var th in trPr.Elements<TableHeader>())
                {
                    var v = th.Val;
                    if (v == null || v.Value == OnOffOnlyValues.On) { isHeader = true; break; }
                }
            }
            rowIsHeader.Add(isHeader);

            var row = new List<IrCell>();
            foreach (var tc in tr.Elements<TableCell>())
            {
                var cell = new IrCell();
                var tcPr = tc.TableCellProperties;
                if (tcPr?.GridSpan?.Val?.Value is int span && span > 1)
                {
                    cell.ColSpan = span;
                }
                var vMerge = tcPr?.VerticalMerge;
                bool vMergeContinue = vMerge != null && (vMerge.Val == null || vMerge.Val.Value == MergedCellValues.Continue);
                bool vMergeRestart = vMerge != null && vMerge.Val?.Value == MergedCellValues.Restart;

                cell.Text = string.Join(Environment.NewLine,
                    tc.Elements<Paragraph>().Select(p =>
                        string.Concat(p.Descendants<Text>().Select(x => x.Text))));

                cell.Style = new IrCellStyle
                {
                    BackgroundHex = NormalizeColor(tcPr?.Shading?.Fill?.Value),
                    HAlign = ParseAlignment(tc.Elements<Paragraph>().FirstOrDefault()?.ParagraphProperties?.Justification?.Val?.Value),
                    VAlign = ParseVAlign(tcPr?.TableCellVerticalAlignment?.Val?.Value),
                    Borders = ParseTcBorders(tcPr?.TableCellBorders),
                };
                cell.Suppressed = vMergeContinue;
                row.Add(cell);

                // 占位 ColSpan-1 个被压制单元格
                for (int i = 1; i < cell.ColSpan; i++)
                {
                    row.Add(new IrCell { Suppressed = true });
                }
                _ = vMergeRestart;
            }
            matrix.Add(row);
        }

        // 第二遍：处理 vMerge，将 Continue 单元格往上找 Restart 累加 RowSpan
        for (int c = 0; c < (matrix.Count == 0 ? 0 : matrix[0].Count); c++)
        {
            int? anchor = null;
            for (int r = 0; r < matrix.Count; r++)
            {
                if (c >= matrix[r].Count) { anchor = null; continue; }
                var cell = matrix[r][c];
                if (cell.Suppressed && anchor.HasValue)
                {
                    matrix[anchor.Value][c].RowSpan++;
                }
                else if (!cell.Suppressed)
                {
                    anchor = r;
                }
            }
        }

        foreach (var rowCells in matrix)
        {
            var row = new IrRow();
            row.Cells.AddRange(rowCells);
            table.Rows.Add(row);
        }

        // 计算 HeaderRowCount：从首行起连续标记为 tblHeader 的行数（DOCX 仅这种连续序列具有跨页重复表头语义）。
        int header = 0;
        for (int i = 0; i < rowIsHeader.Count; i++)
        {
            if (rowIsHeader[i]) header++;
            else break;
        }
        if (header > 0) table.HeaderRowCount = header;

        _ = main;
        return table;
    }

    private static HorizontalAlign ParseAlignment(JustificationValues? j)
    {
        if (j == null) return HorizontalAlign.Left;
        if (j.Value == JustificationValues.Center) return HorizontalAlign.Center;
        if (j.Value == JustificationValues.Right || j.Value == JustificationValues.End) return HorizontalAlign.Right;
        if (j.Value == JustificationValues.Both || j.Value == JustificationValues.Distribute) return HorizontalAlign.Justify;
        return HorizontalAlign.Left;
    }

    private static VerticalAlign ParseVAlign(TableVerticalAlignmentValues? v)
    {
        if (v == null) return VerticalAlign.Top;
        if (v.Value == TableVerticalAlignmentValues.Center) return VerticalAlign.Middle;
        if (v.Value == TableVerticalAlignmentValues.Bottom) return VerticalAlign.Bottom;
        return VerticalAlign.Top;
    }

    private static double? ParseHalfPoint(string? halfPoint)
    {
        if (double.TryParse(halfPoint, out var v)) return v / 2.0;
        return null;
    }

    private static string? NormalizeColor(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (value.Equals("auto", StringComparison.OrdinalIgnoreCase)) return null;
        var v = value.TrimStart('#');
        if (v.Length == 6) return "#" + v.ToUpperInvariant();
        return null;
    }

    private static IrBorders? ParseTcBorders(TableCellBorders? b)
    {
        if (b == null) return null;
        var br = new IrBorders
        {
            Top = ToBorder(b.TopBorder),
            Right = ToBorder(b.RightBorder),
            Bottom = ToBorder(b.BottomBorder),
            Left = ToBorder(b.LeftBorder),
        };
        if (br.Top == null && br.Right == null && br.Bottom == null && br.Left == null) return null;
        return br;
    }

    private static IrBorder? ToBorder(BorderType? bt)
    {
        if (bt == null) return null;
        var val = bt.Val?.Value;
        if (val == BorderValues.None || val == BorderValues.Nil) return null;
        // sz 单位为 1/8 pt
        double thickness = 0.5;
        if (bt.Size?.Value is { } sz) thickness = Math.Max(0.25, sz / 8.0);
        return new IrBorder { Thickness = thickness, ColorHex = NormalizeColor(bt.Color?.Value) ?? "#666666" };
    }
}

/// <summary>缓存 styles.xml + numbering.xml 解析结果，用于样式继承与列表前缀。</summary>
internal sealed class WordContext
{
    private readonly Dictionary<string, Style> _styles;
    private readonly Numbering? _numbering;
    private readonly Dictionary<(int numId, int ilvl), int> _counters = new();
    private readonly MainDocumentPart _main;
    /// <summary>文档级 docDefaults（w:docDefaults/w:rPrDefault + w:pPrDefault）解析结果，作为所有样式链的起点。</summary>
    private readonly ResolvedStyle _docDefaults = new();

    public MainDocumentPart MainPart => _main;

    /// <summary>诊断收集器，可为空。</summary>
    public SnowyRiver.Documents.Converter.Abstractions.ConversionDiagnostics? Diagnostics { get; set; }

    /// <summary>是否启用 Word 字段（影响 TOC 等占位生成）。</summary>
    public bool EnableFields { get; set; } = true;

    /// <summary>解析 hyperlink 关系 ID 为外部 URL；找不到返回 null。</summary>
    public string? ResolveHyperlink(string? relationshipId)
    {
        if (string.IsNullOrEmpty(relationshipId)) return null;
        try
        {
            var rel = _main.HyperlinkRelationships.FirstOrDefault(r => r.Id == relationshipId);
            return rel?.Uri?.ToString();
        }
        catch { return null; }
    }

    public WordContext(MainDocumentPart main)
    {
        _main = main;
        _styles = new Dictionary<string, Style>(StringComparer.Ordinal);
        var sp = main.StyleDefinitionsPart?.Styles;
        if (sp != null)
        {
            foreach (var s in sp.Elements<Style>())
            {
                var id = s.StyleId?.Value;
                if (!string.IsNullOrEmpty(id)) _styles[id] = s;
            }
        }
        _numbering = main.NumberingDefinitionsPart?.Numbering;
        LoadDocDefaults(main.StyleDefinitionsPart?.Styles);
    }

    private void LoadDocDefaults(Styles? styles)
    {
        var dd = styles?.Elements<DocDefaults>().FirstOrDefault();
        if (dd == null) return;
        var rPr = dd.RunPropertiesDefault?.RunPropertiesBaseStyle;
        if (rPr != null)
        {
            _docDefaults.RunDefaults.FontFamily = rPr.RunFonts?.Ascii?.Value
                                                  ?? rPr.RunFonts?.EastAsia?.Value
                                                  ?? _docDefaults.RunDefaults.FontFamily;
            if (rPr.FontSize?.Val?.Value is { } fs && double.TryParse(fs, out var d))
                _docDefaults.RunDefaults.FontSize = d / 2.0;
            if (rPr.Bold != null) _docDefaults.RunDefaults.Bold = true;
            if (rPr.Italic != null) _docDefaults.RunDefaults.Italic = true;
            if (rPr.Underline != null) _docDefaults.RunDefaults.Underline = true;
            var c = rPr.Color?.Val?.Value;
            if (!string.IsNullOrEmpty(c) && !string.Equals(c, "auto", StringComparison.OrdinalIgnoreCase))
            {
                var v = c!.TrimStart('#');
                if (v.Length == 6) _docDefaults.RunDefaults.ColorHex = "#" + v.ToUpperInvariant();
            }
        }
        var pPr = dd.ParagraphPropertiesDefault?.ParagraphPropertiesBaseStyle;
        var sp = pPr?.SpacingBetweenLines;
        if (sp != null)
        {
            if (uint.TryParse(sp.Before?.Value, out var sb)) _docDefaults.SpaceBeforePt = sb / 20.0;
            if (uint.TryParse(sp.After?.Value, out var sa)) _docDefaults.SpaceAfterPt = sa / 20.0;
            if (sp.Line?.Value is { } lineVal && double.TryParse(lineVal, out var lineD))
            {
                var rule = sp.LineRule?.Value;
                if (rule == LineSpacingRuleValues.Exact || rule == LineSpacingRuleValues.AtLeast)
                    _docDefaults.LineHeightPt = lineD / 20.0;
                else
                    _docDefaults.LineHeightRatio = lineD / 240.0;
            }
        }
    }

    private void SeedFromDocDefaults(ResolvedStyle rs)
    {
        rs.RunDefaults.FontFamily ??= _docDefaults.RunDefaults.FontFamily;
        rs.RunDefaults.FontSize ??= _docDefaults.RunDefaults.FontSize;
        if (_docDefaults.RunDefaults.Bold) rs.RunDefaults.Bold = true;
        if (_docDefaults.RunDefaults.Italic) rs.RunDefaults.Italic = true;
        if (_docDefaults.RunDefaults.Underline) rs.RunDefaults.Underline = true;
        rs.RunDefaults.ColorHex ??= _docDefaults.RunDefaults.ColorHex;
        rs.SpaceBeforePt ??= _docDefaults.SpaceBeforePt;
        rs.SpaceAfterPt ??= _docDefaults.SpaceAfterPt;
        rs.LineHeightPt ??= _docDefaults.LineHeightPt;
        rs.LineHeightRatio ??= _docDefaults.LineHeightRatio;
    }

    public ResolvedStyle ResolveParagraphStyle(string? styleId)
    {
        var rs = new ResolvedStyle();
        SeedFromDocDefaults(rs);
        if (string.IsNullOrEmpty(styleId)) return rs;
        ApplyStyleChain(styleId!, rs, paragraphStyle: true);
        return rs;
    }

    public RunStyle ResolveRunStyle(string? runStyleId, RunStyle inherited)
    {
        var rs = new ResolvedStyle { RunDefaults = inherited.Clone() };
        if (!string.IsNullOrEmpty(runStyleId))
            ApplyStyleChain(runStyleId!, rs, paragraphStyle: false);
        return rs.RunDefaults;
    }

    private void ApplyStyleChain(string id, ResolvedStyle target, bool paragraphStyle)
    {
        if (!_styles.TryGetValue(id, out var style)) return;
        var basedOn = style.BasedOn?.Val?.Value;
        if (!string.IsNullOrEmpty(basedOn))
            ApplyStyleChain(basedOn!, target, paragraphStyle);

        if (paragraphStyle)
        {
            var pPr = style.StyleParagraphProperties;
            var j = pPr?.Justification?.Val?.Value;
            if (j != null) target.Alignment = WordReader_AlignFromJ(j.Value);
            var sp = pPr?.SpacingBetweenLines;
            if (sp != null)
            {
                if (uint.TryParse(sp.Before?.Value, out var sb)) target.SpaceBeforePt = sb / 20.0;
                if (uint.TryParse(sp.After?.Value, out var sa)) target.SpaceAfterPt = sa / 20.0;
                if (sp.Line?.Value is { } lineVal && double.TryParse(lineVal, out var lineD))
                {
                    var rule = sp.LineRule?.Value;
                    if (rule == LineSpacingRuleValues.Exact || rule == LineSpacingRuleValues.AtLeast)
                        target.LineHeightPt = lineD / 20.0;
                    else
                        target.LineHeightRatio = lineD / 240.0;
                }
            }
        }

        var rPr = style.StyleRunProperties;
        if (rPr != null)
        {
            target.RunDefaults.FontFamily = rPr.RunFonts?.Ascii?.Value
                                            ?? rPr.RunFonts?.EastAsia?.Value
                                            ?? target.RunDefaults.FontFamily;
            if (rPr.FontSize?.Val?.Value is { } fs && double.TryParse(fs, out var d))
                target.RunDefaults.FontSize = d / 2.0;
            if (rPr.Bold != null) target.RunDefaults.Bold = true;
            if (rPr.Italic != null) target.RunDefaults.Italic = true;
            if (rPr.Underline != null) target.RunDefaults.Underline = true;
            var c = rPr.Color?.Val?.Value;
            if (!string.IsNullOrEmpty(c) && !string.Equals(c, "auto", StringComparison.OrdinalIgnoreCase))
            {
                var v = c!.TrimStart('#');
                if (v.Length == 6) target.RunDefaults.ColorHex = "#" + v.ToUpperInvariant();
            }
        }
    }

    private static HorizontalAlign WordReader_AlignFromJ(JustificationValues v)
    {
        if (v == JustificationValues.Center) return HorizontalAlign.Center;
        if (v == JustificationValues.Right || v == JustificationValues.End) return HorizontalAlign.Right;
        if (v == JustificationValues.Both || v == JustificationValues.Distribute) return HorizontalAlign.Justify;
        return HorizontalAlign.Left;
    }

    /// <summary>解析段落对应的列表信息（类型/层级）。无列表时返回 null。不修改计数器。</summary>
    public (ListType Type, int Level)? ResolveListInfo(Paragraph p)
    {
        if (_numbering == null) return null;
        var numPr = p.ParagraphProperties?.NumberingProperties;
        if (numPr == null) return null;
        var numIdVal = numPr.NumberingId?.Val?.Value;
        var ilvlVal = numPr.NumberingLevelReference?.Val?.Value ?? 0;
        if (!numIdVal.HasValue) return null;
        int numId = numIdVal.Value;
        int ilvl = ilvlVal;

        var numEl = _numbering.Elements<NumberingInstance>()
            .FirstOrDefault(n => n.NumberID?.Value == numId);
        var absId = numEl?.AbstractNumId?.Val?.Value;
        if (!absId.HasValue) return null;

        var abs = _numbering.Elements<AbstractNum>()
            .FirstOrDefault(a => a.AbstractNumberId?.Value == absId.Value);
        var lvl = abs?.Elements<Level>().FirstOrDefault(l => l.LevelIndex?.Value == ilvl);
        if (lvl == null) return null;

        var fmt = lvl.NumberingFormat?.Val?.Value;
        if (fmt == NumberFormatValues.Bullet) return (ListType.Bullet, ilvl);

        ListType lt = fmt switch
        {
            var f when f == NumberFormatValues.Decimal => ListType.Decimal,
            var f when f == NumberFormatValues.LowerLetter => ListType.LowerLetter,
            var f when f == NumberFormatValues.UpperLetter => ListType.UpperLetter,
            var f when f == NumberFormatValues.LowerRoman => ListType.LowerRoman,
            var f when f == NumberFormatValues.UpperRoman => ListType.UpperRoman,
            _ => ListType.Decimal,
        };
        return (lt, ilvl);
    }

    /// <summary>根据段落 numPr / numbering.xml 计算项目符号或编号文字（不支持时返回 null）。</summary>
    public string? ResolveListPrefix(Paragraph p)
    {
        if (_numbering == null) return null;
        var numPr = p.ParagraphProperties?.NumberingProperties;
        if (numPr == null) return null;
        var numIdVal = numPr.NumberingId?.Val?.Value;
        var ilvlVal = numPr.NumberingLevelReference?.Val?.Value ?? 0;
        if (!numIdVal.HasValue) return null;
        int numId = numIdVal.Value;
        int ilvl = ilvlVal;

        // num -> abstractNumId
        var numEl = _numbering.Elements<NumberingInstance>()
            .FirstOrDefault(n => n.NumberID?.Value == numId);
        var absId = numEl?.AbstractNumId?.Val?.Value;
        if (!absId.HasValue) return null;

        var abs = _numbering.Elements<AbstractNum>()
            .FirstOrDefault(a => a.AbstractNumberId?.Value == absId.Value);
        var lvl = abs?.Elements<Level>().FirstOrDefault(l => l.LevelIndex?.Value == ilvl);
        if (lvl == null) return null;

        var fmt = lvl.NumberingFormat?.Val?.Value;
        var text = lvl.LevelText?.Val?.Value ?? string.Empty;

        if (fmt == NumberFormatValues.Bullet)
        {
            // 简化：常见项目符号统一展示为 "• "
            return "• ";
        }

        // 自增
        var key = (numId, ilvl);
        _counters.TryGetValue(key, out var cur);
        cur++;
        _counters[key] = cur;
        // 重置更深层级
        var keys = _counters.Keys.Where(k => k.numId == numId && k.ilvl > ilvl).ToList();
        foreach (var k in keys) _counters.Remove(k);

        // 替换 %N 占位（%1..%9 对应 ilvl=0..8 的当前计数值，遵循各层自身的 numFmt）
        string body = text;
        for (int i = 1; i <= 9; i++)
        {
            string token = "%" + i;
            if (!body.Contains(token)) continue;
            int parentIlvl = i - 1;
            int valForLevel;
            NumberFormatValues? fmtForLevel;
            if (parentIlvl == ilvl)
            {
                valForLevel = cur;
                fmtForLevel = fmt;
            }
            else
            {
                valForLevel = _counters.TryGetValue((numId, parentIlvl), out var v) ? v : 1;
                var parentLvl = abs?.Elements<Level>().FirstOrDefault(l => l.LevelIndex?.Value == parentIlvl);
                fmtForLevel = parentLvl?.NumberingFormat?.Val?.Value ?? fmt;
            }
            body = body.Replace(token, FormatNumber(valForLevel, fmtForLevel));
        }
        if (string.IsNullOrEmpty(body)) body = cur.ToString();
        return body + " ";
    }

    private static string FormatNumber(int n, NumberFormatValues? fmt)
    {
        if (fmt == NumberFormatValues.LowerLetter) return ToLetter(n, lower: true);
        if (fmt == NumberFormatValues.UpperLetter) return ToLetter(n, lower: false);
        if (fmt == NumberFormatValues.LowerRoman) return ToRoman(n).ToLowerInvariant();
        if (fmt == NumberFormatValues.UpperRoman) return ToRoman(n);
        return n.ToString();
    }

    private static string ToLetter(int n, bool lower)
    {
        if (n <= 0) return string.Empty;
        var sb = new System.Text.StringBuilder();
        while (n > 0)
        {
            n--;
            sb.Insert(0, (char)((lower ? 'a' : 'A') + (n % 26)));
            n /= 26;
        }
        return sb.ToString();
    }

    private static string ToRoman(int n)
    {
        if (n <= 0) return n.ToString();
        var map = new (int v, string s)[]
        {
            (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
            (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
            (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I"),
        };
        var sb = new System.Text.StringBuilder();
        foreach (var (v, s) in map)
            while (n >= v) { sb.Append(s); n -= v; }
        return sb.ToString();
    }
}

/// <summary>承载样式继承结果。</summary>
internal sealed class ResolvedStyle
{
    public HorizontalAlign Alignment { get; set; } = HorizontalAlign.Left;
    public RunStyle RunDefaults { get; set; } = new();
    public double? SpaceBeforePt { get; set; }
    public double? SpaceAfterPt { get; set; }
    public double? LineHeightPt { get; set; }
    public double? LineHeightRatio { get; set; }
}

/// <summary>承载 Run 级别的解析后属性。</summary>
internal sealed class RunStyle
{
    public string? FontFamily { get; set; }
    public double? FontSize { get; set; }
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public string? ColorHex { get; set; }

    public RunStyle Clone() => new()
    {
        FontFamily = FontFamily,
        FontSize = FontSize,
        Bold = Bold,
        Italic = Italic,
        Underline = Underline,
        ColorHex = ColorHex,
    };
}
