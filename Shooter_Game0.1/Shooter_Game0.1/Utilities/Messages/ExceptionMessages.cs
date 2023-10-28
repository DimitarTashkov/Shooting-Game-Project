﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shooter_Game0._1.Utilities.Messages
{
    public static class ExceptionMessages
    {
        public static string EmptyName = "Our weapons and enemies must have a name";
        public static string InvalidWeaponsStats = "Our weapons must have ammo and power values";
        public static string IvalidHealthValue = "size and health must be bigger than 0";
        public static string InvalidCoordinates = "map coordinates must be in range of 1 and 10";
        public static string EnemyHasAlreadyBeenRegenerated = "{0} has already been regenerated";
        public static string MapRowsOrColsShouldNotBeEmpty = "Map should have valid rows and cols";
    }
}