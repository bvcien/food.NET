using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NETCORE.Utils.Constants;

namespace NETCORE.Data.Entities
{
    public class UserSetting
    {
        public UserSetting()
        {
            Theme = Theme.light;
            Direction = Direction.ltr;
            ColorTheme = ColorTheme.Purple_Theme;
            Layout = Layout.vertical;
            BoxedLayout = true;
            SidebarType = SidebarType.mini;
            CardBorder = false;
        }
        public int Id { get; set; }
        [MaxLength(50)]
        [Column(TypeName = "varchar(50)")]
        public string? UserId { get; set; }
        public User? User { get; set; }

        public Theme Theme { get; set; }
        public Direction Direction { get; set; }
        public ColorTheme ColorTheme { get; set; }
        public Layout Layout { get; set; }
        public bool BoxedLayout { get; set; }
        public SidebarType SidebarType { get; set; }
        public bool CardBorder { get; set; }
    }
}