using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UserEditorViewModelBase<
    TUser, TUserEntity, 
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TUser, TUserEntity>(unitOfWorkFactory, mapper, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity: SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRole : Role<TUser, TRole, TTeam, TPermission>, new()
    where TTeamEntity : SnowyRiver.Accounts.Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>, new()
    where TPermissionEntity : SnowyRiver.Accounts.Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>, new()
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            if (navigationContext.Parameters.TryGetValue<bool>(nameof(TeamsEnable), out var teamsEnable))
            {
                TeamsEnable = teamsEnable;
            }

            base.OnNavigatedTo(navigationContext);

            using var unitOfWork = UnitOfWorkFactory.Create();
            var roleRepository = unitOfWork.Repository<TRoleEntity>();
            var roleQuery = roleRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var roles = await roleRepository.SearchAsync(roleQuery);
            if (roles != null)
            {
                _roleEntities = roles;
                Roles = await Mapper.From(_roleEntities)
                    .AdaptToTypeAsync<ObservableCollection<TRole>>();
                foreach (var role in Roles)
                {
                    role.IsSelected = Model.Roles.Any(x => x.Id ==  role.Id);
                }
            }

            var teamRepository = unitOfWork.Repository<TTeamEntity>();
            var teamQuery = teamRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var teams = await teamRepository.SearchAsync(teamQuery);
            if (teams != null)
            {
                _teamEntities = teams;
                Teams = await Mapper.From(_teamEntities)
                    .AdaptToTypeAsync<ObservableCollection<TTeam>>();
                foreach (var team in Teams)
                {
                    team.IsSelected = Model.Roles.Any(x => x.Id == team.Id);
                }
            }
        }
        catch (Exception e)
        {
            //
        }
    }

    protected async Task<TUserEntity> MapToEntityAsync(TUser model, IUnitOfWork unitOfWork)
    {
        var entity =  await Mapper.From(model)
            .AdaptToTypeAsync<TUserEntity>();
        await MapToEntityAsync(entity);
        if (string.IsNullOrWhiteSpace(entity.PasswordSalt))
        {
            entity.PasswordSalt = PasswordHelper.CreateSalt();
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            entity.Password = PasswordHelper.CreatePassword(model.NewPassword, entity.PasswordSalt);
        }

        return entity;
    }


    protected async Task MapToEntityAsync(TUser model, TUserEntity entity, IUnitOfWork unitOfWork)
    {
//        await base.MapToEntityAsync(model, entity, unitOfWork);
        entity.Name = model.Name;
        if (string.IsNullOrWhiteSpace(entity.PasswordSalt))
        {
            entity.PasswordSalt = PasswordHelper.CreateSalt();
        }

        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            entity.Password = PasswordHelper.CreatePassword(model.NewPassword, entity.PasswordSalt);
        }
        await MapToEntityAsync(entity);
    }

    private async Task MapToEntityAsync(TUserEntity entity)
    {
        entity.Roles.Clear();
        foreach (var role in Roles)
        {
            if (role.IsSelected)
            {
                var roleEntity = _roleEntities.First(x => x.Id == role.Id);
                if (entity.Roles.All(x => x.Id != roleEntity.Id))
                {
                    entity.Roles.Add(roleEntity);
                }

                if (roleEntity.Users.All(x => x.Id != entity.Id))
                {
                    roleEntity.Users.Add(entity);
                }
            }
        }

        if (entity.UserId == 0)
        {
            using var unitOfWork = UnitOfWorkFactory.Create();
            var repository = unitOfWork.Repository<TUserEntity>();
            var maxUserId = await repository.MaxAsync(x => x.UserId);
            entity.UserId = maxUserId + 1;
        }
        await Task.CompletedTask;
    }

    public bool TeamsEnable
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    private IList<TRoleEntity> _roleEntities = [];

    public ObservableCollection<TRole> Roles
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    private IList<TTeamEntity> _teamEntities = [];

    public ObservableCollection<TTeam> Teams
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
