﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter_Game0._1.Models.Enemies.Contracts
{
    public interface IEnemy
    {
       public int EnemySize { get; }
        public double EnemyHealth { get; }
        public double Life { get; }
        public bool IsAlreadyGenerated { get; }
        public void CalculateLife();
       public abstract string RegenHealth();

    }
}
