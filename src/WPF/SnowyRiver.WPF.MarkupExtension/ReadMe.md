# 推荐 XAML 用法模板
1. 先声明命名空间（示例）：

`
xmlns:markup="clr-namespace:SnowyRiver.WPF.MarkupExtension;assembly=SnowyRiver.WPF.MarkupExtension"
`

A 普通枚举直接绑定（显示枚举名）

`
ItemsSource="{markup:EnumBindingSource EnumType={x:Type local:OrderStatus}}"
SelectedItem="{Binding Status}"
`

B 可空枚举绑定（包含空项）

`
ItemsSource="{markup:EnumBindingSource EnumType={x:Type local:OrderStatus}, IncludeNullItem=True}"
SelectedItem="{Binding Status}"
`

C 显示 Description（推荐用于业务下拉）

`
ItemsSource="{markup:EnumerationDescription EnumType={x:Type local:OrderStatus}, IncludeNullItem=True, NullItemDescription=请选择}"
DisplayMemberPath="Description" SelectedValuePath="Value"
 SelectedValue="{Binding Status}"
 `