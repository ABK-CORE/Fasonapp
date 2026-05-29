using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class UserBriefDto : IDto
    {
        public int Id { get; set; }
        public Guid Guid { get; set; }
        public string Username { get; set; } = null!;
    }
}
