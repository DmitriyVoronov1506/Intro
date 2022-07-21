using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApplicationClassWork.DAL.Entities;

namespace WebApplicationClassWork.DAL.Context
{
    public class UsersConfiguration : IEntityTypeConfiguration<Entities.User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(new Entities.User
            {
                Id = System.Guid.NewGuid(),
                RealName = "Корневой администратор",
                Login  = "Admin",
                PassHash = "",
                Email = "",
                PassSalt = "",
                RegMoment = System.DateTime.Now,
                Avatar = ""
            });
        }
    }
}
