using Csla;
using Csla.Data;
using System;

namespace ProjectTracker.Library
{
  namespace Admin
  {
    /// <summary>
    /// Used to maintain the list of roles
    /// in the system.
    /// </summary>
    [Serializable()]
    public class Roles : BusinessListBase<Roles, Role>
    {

      #region  Business Methods

      /// <summary>
      /// Remove a role based on the role's
      /// id value.
      /// </summary>
      /// <param name="id">Id value of the role to remove.</param>
      public void Remove(int id)
      {
        foreach (Role item in this)
        {
          if (item.Id == id)
          {
            Remove(item);
            break;
          }
        }
      }

      /// <summary>
      /// Get a role bsaed on its id value.
      /// </summary>
      /// <param name="id">Id value of the role to return.</param>
      public Role GetRoleById(int id)
      {
        foreach (Role item in this)
        {
          if (item.Id == id)
          {
            return item;
          }
        }
        return null;
      }

      protected override object AddNewCore()
      {
        Role item = Role.NewRole();
        Add(item);
        return item;
      }

      #endregion

      #region  Authorization Rules

      public static bool CanAddObject()
      {
        return Csla.ApplicationContext.User.IsInRole("Administrator");
      }

      public static bool CanGetObject()
      {
        return true;
      }

      public static bool CanDeleteObject()
      {
        bool result = false;
        if (Csla.ApplicationContext.User.IsInRole("Administrator"))
          result = true;
        return result;
      }

      public static bool CanEditObject()
      {
        return Csla.ApplicationContext.User.IsInRole("Administrator");
      }

      #endregion

      #region  Factory Methods

      public static Roles GetRoles()
      {
        return DataPortal.Fetch<Roles>();
      }

      private Roles()
      {
        this.Saved += new EventHandler<Csla.Core.SavedEventArgs>(Roles_Saved);
        this.AllowNew = true;
      }

      #endregion

      #region  Data Access

      public override Roles Save()
      {
        // see if save is allowed
        if (!(CanEditObject()))
          throw new System.Security.SecurityException("User not authorized to save roles");
        return base.Save();
      }

      private void Roles_Saved(object sender, Csla.Core.SavedEventArgs e)
      {
        // this runs on the client and invalidates
        // the RoleList cache
        RoleList.InvalidateCache();
      }

      protected override void DataPortal_OnDataPortalInvokeComplete(Csla.DataPortalEventArgs e)
      {
        if (ApplicationContext.ExecutionLocation == ApplicationContext.ExecutionLocations.Server &&
            e.Operation == DataPortalOperations.Update)
        {
          // this runs on the server and invalidates
          // the RoleList cache
          RoleList.InvalidateCache();
        }
      }

      private void DataPortal_Fetch()
      {
        this.RaiseListChangedEvents = false;
        using (var ctx = ContextManager<ProjectTracker.DalLinq.PTrackerDataContext>.GetManager(Database.PTrackerConnection))
        {
          foreach (var value in ctx.DataContext.getRoles())
            this.Add(Role.GetRole(value));
        }
        this.RaiseListChangedEvents = true;
      }

      [Transactional(TransactionalTypes.TransactionScope)]
      protected override void DataPortal_Update()
      {
        this.RaiseListChangedEvents = false;
        using (var ctx = ContextManager<ProjectTracker.DalLinq.PTrackerDataContext>.GetManager(Database.PTrackerConnection))
        {
          Child_Update();
        }
        this.RaiseListChangedEvents = true;
      }

      #endregion

    }
  }
}