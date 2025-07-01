using Microsoft.AspNetCore.Identity;

namespace PredatorsGym.Datos
{
    public static class InicializadorRoles
    {
        public static async Task CrearRolesIniciales(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = { "Administrador", "UsuarioSinMembresia", "UsuarioConMembresia", "UsuarioPremium" };

            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    await roleManager.CreateAsync(new IdentityRole(rol));
                }
            }
        }
    }
}
