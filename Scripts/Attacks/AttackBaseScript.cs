using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJamBulletHell.Scripts.Attacks
{
    public partial class AttackBaseScript : Godot.Node2D
    {
        public virtual int Damage { get { return 1; } }
    }
}
