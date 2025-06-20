using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Wangkanai.Planet.Portal.Data;

public class PlanetDbContext(DbContextOptions<PlanetDbContext> options) : IdentityDbContext<PlanetUser>(options)
{
}
