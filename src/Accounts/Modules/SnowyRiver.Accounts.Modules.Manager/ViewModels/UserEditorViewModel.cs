using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UserEditorViewModel(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<User, UserEntity>(unitOfWorkFactory, mapper, regionManager)
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
            var roleRepository = unitOfWork.Repository<RoleEntity>();
            var roleQuery = roleRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var roles = await roleRepository.SearchAsync(roleQuery);
            if (roles != null)
            {
                _roleEntities = roles;
                Roles = await Mapper.From(_roleEntities)
                    .AdaptToTypeAsync<ObservableCollection<Role>>();
                foreach (var role in Roles)
                {
                    role.IsSelected = Model.Roles.Any(x => x.Id ==  role.Id);
                }
            }

            var teamRepository = unitOfWork.Repository<TeamEntity>();
            var teamQuery = teamRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var teams = await teamRepository.SearchAsync(teamQuery);
            if (teams != null)
            {
                _teamEntities = teams;
                Teams = await Mapper.From(_teamEntities)
                    .AdaptToTypeAsync<ObservableCollection<Team>>();
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

    protected override async Task<UserEntity> MapToEntityAsync(User model)
    {
        var entity =  await Mapper.From(model)
            .AdaptToTypeAsync<UserEntity>();
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


    protected override async Task MapToEntityAsync(User model, UserEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
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

    private async Task MapToEntityAsync(UserEntity entity)
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
            var repository = unitOfWork.Repository<UserEntity>();
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

    private IList<RoleEntity> _roleEntities = [];

    public ObservableCollection<Role> Roles
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    private IList<TeamEntity> _teamEntities = [];

    public ObservableCollection<Team> Teams
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
