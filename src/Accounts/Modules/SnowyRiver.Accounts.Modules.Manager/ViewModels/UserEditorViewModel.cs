using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UserEditorViewModel(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<User, UserEntity>(unitOfWork, mapper, regionManager)
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

            var roleRepository = UnitOfWork.Repository<RoleEntity>();
            var roleQuery = roleRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var roles = await roleRepository.SearchAsync(roleQuery);
            if (roles != null)
            {
                _roleEntities = roles;
                Roles = Mapper.Map<ObservableCollection<Role>>(_roleEntities);
                foreach (var role in Roles)
                {
                    role.IsSelected = Model.Roles.Any(x => x.Id ==  role.Id);
                }
            }

            var teamRepository = UnitOfWork.Repository<TeamEntity>();
            var teamQuery = teamRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var teams = await teamRepository.SearchAsync(teamQuery);
            if (teams != null)
            {
                _teamEntities = teams;
                Teams = Mapper.Map<ObservableCollection<Team>>(_teamEntities);
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
        var entity =  await Task.FromResult(Mapper.Map<UserEntity>(model));
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
        entity.Roles ??= [];
        entity.Roles.Clear();
        foreach (var role in Roles)
        {
            if (role.IsSelected)
            {
                var roleEntity = _roleEntities.First(x => x.Id == role.Id);
                entity.Roles ??= [];
                if (entity.Roles.All(x => x.Id != roleEntity.Id))
                {
                    entity.Roles.Add(roleEntity);
                }

                roleEntity.Users??= [];
                if (roleEntity.Users.All(x => x.Id != entity.Id))
                {
                    roleEntity.Users.Add(entity);
                }
            }
        }

        if (entity.UserId == 0)
        {
            var repository = UnitOfWork.Repository<UserEntity>();
            var maxUserId = await repository.MaxAsync(x => x.UserId);
            entity.UserId = maxUserId + 1;
        }
        await Task.CompletedTask;
    }

    private bool _teamsEnable = true;
    public bool TeamsEnable
    {
        get => _teamsEnable;
        set => SetProperty(ref _teamsEnable, value);
    }

    private IList<RoleEntity> _roleEntities = [];

    private ObservableCollection<Role> _roles = [];
    public ObservableCollection<Role> Roles
    {
        get => _roles;
        set => SetProperty(ref _roles, value);
    }

    private IList<TeamEntity> _teamEntities = [];

    private ObservableCollection<Team> _teams = [];
    public ObservableCollection<Team> Teams
    {
        get => _teams;
        set => SetProperty(ref _teams, value);
    }
}
