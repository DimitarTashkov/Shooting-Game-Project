﻿using Shooter_Game0._1.Core.Contracts;
using Shooter_Game0._1.IO;
using Shooter_Game0._1.Models.Enemies.Contracts;
using Shooter_Game0._1.Models.Map.Contracts;
using Shooter_Game0._1.Models.Users.Contracts;
using Shooter_Game0._1.Models.Weapons.Contracts;
using Shooter_Game0._1.Repositories;
using Shooter_Game0._1.Utilities.Hinter;
using Shooter_Game0._1.Utilities.Messages;
using Shooter_Game0._1.Utilities.Randomizer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shooter_Game0._1.Core
{
    public class Controller : IController
    {
        private double collectDealtDamage;
        private int collectKills;

        private int oldXCoordinate;
        private int oldYCoordinate;

        private EnemiesRepository enemies;
        private WeaponsRepository weapons;
        private MapsRepository maps;
        private UsersRepository users;
        private DataBuilder builder;
        private StringBuilder sb;
        private EnemiesCoordinatesRepository enemiesCoordinates;
        private Writer writer;
        public Controller()
        {
            this.enemies = new EnemiesRepository();
            this.weapons = new WeaponsRepository();
            this.maps = new MapsRepository();
            this.users = new UsersRepository();
            this.builder = new DataBuilder();
            this.sb = new StringBuilder();
            this.writer = new Writer();
            enemiesCoordinates = new EnemiesCoordinatesRepository();
        }
        public Dictionary<Dictionary<int, int>, IEnemy> EnemiesCoordinates => enemiesCoordinates.Enemiescoordinates;
        public string GenerateEnemies(IMap map, int countOfEnemies)
        {
            map.GenerateTerrain();
            for (int i = 0; i < countOfEnemies; i++)
            {
                IEnemy generatedEnemy = Randomizer.EnemiesRandomizer();
                enemies.AddNew(generatedEnemy);
                Dictionary<int, int> enemyCoordinates = Randomizer.EnemiesGenerationRandomizer(map);
                while (map.CoordinateIsAlreadyInhabitated(enemyCoordinates, enemiesCoordinates.Enemiescoordinates)) 
                {
                    enemyCoordinates = Randomizer.EnemiesGenerationRandomizer(map);
                }

                if(!map.CoordinateIsAlreadyInhabitated(enemyCoordinates, enemiesCoordinates.Enemiescoordinates))
                {                                                             
                    enemiesCoordinates.AddEnemy(enemyCoordinates, generatedEnemy);
                }                
               
            }
            maps.AddNew(map);
            map.VisualizeMap(map.Terrain);

            sb.AppendLine(string.Format(OutputMessages.MapWasAdded, map.GetType().Name));
            sb.AppendLine(string.Format(OutputMessages.EnemiesGenerated, countOfEnemies, map.GetType().Name));
            return sb.ToString().Trim();
        }

        public string Shoot(int xCoordinate, int yCoordinate)
        {

            sb.Clear();
            string returnInfo = string.Empty;


            Dictionary<int, int> aimCoordinates = new Dictionary<int, int>();
            aimCoordinates.Add(xCoordinate, yCoordinate);

            IWeapon weapon = Randomizer.WeaponsRandomizer();
            IMap map = maps.Models().FirstOrDefault();
            IEnemy enemy = ReturnEnemyFromCoordinates(xCoordinate,yCoordinate,enemiesCoordinates.Enemiescoordinates);
            weapons.AddNew(weapon);

            map.Terrain[oldXCoordinate, oldYCoordinate] = "-";
            oldXCoordinate = xCoordinate;
            oldYCoordinate = yCoordinate;

            if (map == null)
            {
                throw new InvalidOperationException(string.Format(ExceptionMessages.MapHasNotBeenAdded, map.GetType().Name));
            }      
            
            if (enemy == null)
            {
                writer.WriteLine(Environment.NewLine);
                map.Terrain[xCoordinate, yCoordinate] = "+";
                map.VisualizeMap(map.Terrain);
                returnInfo = string.Format(OutputMessages.NoEnemyInThisLocation, xCoordinate, yCoordinate);
                return returnInfo;
            }
                weapon.CalculateDamage();
                double remain = weapon.Damage - enemy.Life;
                if (remain <= 0)
                {
                collectDealtDamage += weapon.Damage;
                    enemy.Life -= weapon.Damage;
                    sb.AppendLine(string.Format(OutputMessages.EnemyWasShotFor, enemy.GetType().Name, weapon.Damage, weapon.GetType().Name,xCoordinate,yCoordinate));
                    sb.AppendLine(enemy.RegenHealth());
                enemiesCoordinates.RemoveEnemy(aimCoordinates);
                    enemy.RunCoordinates(map, enemy, enemiesCoordinates.Enemiescoordinates);                   

                }
                else
                {
                collectDealtDamage += weapon.Damage;
                enemiesCoordinates.RemoveEnemy(aimCoordinates);
                    collectKills++;
                    sb.AppendLine(string.Format(OutputMessages.EnemyWasKilled, enemy.GetType().Name, weapon.GetType().Name,xCoordinate,yCoordinate,enemiesCoordinates.Enemiescoordinates.Count));
                    enemy.IsEnemyKilled = true;
                    
                }
            map.Terrain[xCoordinate, yCoordinate] = "+";
            map.VisualizeMap(map.Terrain);
            return sb.ToString().Trim();
        }

        public void StatsUpdate(string username)
        {
            sb.Clear();
            IUser user = null;
            if(users.Models().Any(u => u.Username == username))
            {
                 user = users.Models().FirstOrDefault(u => u.Username == username);
            }
            else
            {
                user = builder.CreateUser(username);
                users.AddNew(user);
            }
            user.DamageDealt += collectDealtDamage;
            user.EnemiesKilled += collectKills;
            user.Points = (user.EnemiesKilled * 300) + (user.DamageDealt / 3);
            sb.AppendLine(string.Format(OutputMessages.UserReport, username, user.DamageDealt, user.EnemiesKilled, Math.Round(user.Points,2)));
        }
        public void Report()
        {
            writer.WriteLine(sb.ToString().Trim());
        }
        private IEnemy ReturnEnemyFromCoordinates(int x,int y, Dictionary<Dictionary<int, int>, IEnemy> enemiesCoordinates)
        {
            Dictionary<int, int> aimCoordinates = new Dictionary<int, int>();
            aimCoordinates.Add(x, y);

            IEnemy enemy = null;
            foreach (var kvp in enemiesCoordinates)
            {
                Dictionary<int, int> coordinate = kvp.Key;

                // Check if the current dictionary entry's coordinates match the target coordinates
                if (coordinate.ContainsKey(x) && coordinate[x] == y)
                {
                    enemy = kvp.Value; // Assign the matching IEnemy to the 'enemy' variable
                    break; // Exit the loop since we found the enemy
                }
            }
            return enemy;
        }

        public string Hint(int xCoordinate, int yCoordinate, string[,] terrain, Dictionary<Dictionary<int, int>, IEnemy> enemiesCoordinates)
        {
            sb.Clear();
            string closestEnemy = Hinter.GetHint(xCoordinate, yCoordinate, terrain, enemiesCoordinates);
            sb.AppendLine(closestEnemy);
            return sb.ToString().Trim();
        }
    }
}
