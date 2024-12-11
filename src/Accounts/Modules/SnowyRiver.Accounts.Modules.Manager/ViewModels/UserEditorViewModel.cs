using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
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
        return entity;
    }


    protected override async Task MapToEntityAsync(User model, UserEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
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

        await Task.CompletedTask;
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
