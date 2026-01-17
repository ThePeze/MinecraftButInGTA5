using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using GTA.Native;

/*
Sorry in advance for anyone trying to read through this code, it's very poorly written
and not commented well. Feel free to modify anything you want.
*/


namespace GTA5TestMod
{
    public class TestMod : Script
    {
        private int lastCheck;
        private int lastCheckSeconds;
        private int lastCheckCreeperSpawn;
        private int lastCheckSkeletonSpawn;
        private Ped Zombie;
        private Ped Creeper;
        private Ped Skeleton;
        private List<Ped> SpawnedZombies = new List<Ped>();
        private List<Ped> SpawnedCreepers = new List<Ped>();
        private List<Ped> SpawnedSkeletons = new List<Ped>();
        private Vector3 lastPlayerPosition = Vector3.Zero;
        private Random rnd = new Random();
        int zombieCounter = 0;
        int creeperCounter = 0;
        int skeletonCounter = 0;
        private Vehicle car;
        private int checkIntervall = 1000;
        private const int secondsIntervall = 1000;
        private const int CreeperSpawnIntervall = 5000;
        private const int SkeletonSpawnIntervall = 10000;


        public TestMod() {
            Tick += OnTick;
            KeyUp += OnKeyUp;
            KeyDown += OnKeyDown;
            lastCheck = Game.GameTime;
            lastCheckCreeperSpawn = Game.GameTime;
            lastCheckSeconds = Game.GameTime;
        }

        private void OnTick(object sender, EventArgs e)
        {
            //zomibe intervall
            if (Game.GameTime - lastCheck > checkIntervall)
            {
                lastCheck = Game.GameTime;
                int CurrentHour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
		//Display current time
                //GTA.UI.Notification.PostTicker("Current Time " + CurrentHour + ":" + Function.Call<int>(Hash.GET_CLOCK_MINUTES), false);
                if (CurrentHour > 20 || CurrentHour < 6)
                {
                    Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, Game.Player); // Never Wanted
                    for (int i = SpawnedZombies.Count - 1; i >= 0; i--)
                    {
                        // Check that it won't go under 0 !!!!!, i didn't end up doing this but who really cares, it doesn't matter
                        Ped z = SpawnedZombies[i];
                        if (z == null || !z.Exists())
                        {
                            SpawnedZombies.RemoveAt(i);
                            zombieCounter--;
                            continue;
                        }
                        bool zombieDead = Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, z, 1);
                        if (zombieDead)
                        {
                            z.Delete();
                            zombieCounter--;
                            continue;
                        }
                        z.Task.Combat(Game.Player.Character);
                    }
                    if (zombieCounter < 100)
                    {
                        float randomX = RandomFloat(5, 25);
                        float randomY = RandomFloat(5, 25);

                        GTA.UI.Notification.PostTicker("Spawning " + zombieCounter + ". Zombie", false);

                        int plusMinus = (int)RandomFloat(0, 1); // 0 = + ; 1 = - 
                        if(plusMinus == 0)
                        {
                            Zombie = World.CreatePed(PedHash.Zombie01, Game.Player.Character.GetOffsetPosition(new Vector3(randomX, randomY, 0)));
                            Zombie.DrownsInWater = true;
                            Zombie.Task.Combat(Game.Player.Character);
                            zombieCounter++;
                            SpawnedZombies.Add(Zombie);
                        }
                        else if(plusMinus == 1)
                        {
                            Zombie = World.CreatePed(PedHash.Zombie01, Game.Player.Character.GetOffsetPosition(new Vector3(-randomX, -randomY, 0)));
                            Zombie.DrownsInWater = true;
                            Zombie.Task.Combat(Game.Player.Character);
                            zombieCounter++;
                            SpawnedZombies.Add(Zombie);
                        }
                        else
                        {
                            GTA.UI.Notification.PostTicker("plusminus returned wrong number: " + plusMinus, false);
                        }                            
                    }
                    switch (zombieCounter)
                    {
                        case 0:
                            checkIntervall = 1000;
                            break;
                        case 5:
                            checkIntervall = 2000;
                            break;
                        case 10:
                            checkIntervall = 3000;
                            break;
                        case 20:
                            checkIntervall = 5000;
                            break;
                        case 50:
                            checkIntervall = 10000;
                            break;
                    }
                }
                else if (CurrentHour > 5 && CurrentHour < 21)
                {
                    for (int i = SpawnedZombies.Count - 1; i >= 0; i--)
                    {
                        Ped z = SpawnedZombies[i];
                        if (z == null || !z.Exists())
                        {
                            SpawnedZombies.RemoveAt(i);
                            zombieCounter--;
                            continue;
                        }
                        z.Delete();
                        zombieCounter--;
                    }
                    if (checkIntervall != 1000) checkIntervall = 1000;
                }

            }
            //Gets called every second
            if (Game.GameTime - lastCheckSeconds > secondsIntervall)
            {
                lastCheckSeconds = Game.GameTime;
                CreeperChasePlayer();
                SkeletonShootPlayer();
            }
            if(Game.GameTime - lastCheckCreeperSpawn > CreeperSpawnIntervall)
            {
                lastCheckCreeperSpawn = Game.GameTime;
                int CurrentHour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
                HandleCreepers(CurrentHour);
            }
            if (Game.GameTime - lastCheckSkeletonSpawn > SkeletonSpawnIntervall)
            {
                lastCheckSkeletonSpawn = Game.GameTime;
                int CurrentHour = Function.Call<int>(Hash.GET_CLOCK_HOURS);
                HandleSkeletons(CurrentHour);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.U) {
                float randomX = RandomFloat(5, 25);
                float randomY = RandomFloat(5, 25);
                GTA.UI.Notification.PostTicker("Manually Spawning Zombie", false);

                Zombie = World.CreatePed(PedHash.Zombie01, Game.Player.Character.GetOffsetPosition(new Vector3(randomX, randomY, 0)));
                Zombie.Task.Combat(Game.Player.Character);
            }
            
            if(e.KeyCode == Keys.O){
                car = World.CreateVehicle(VehicleHash.Emerus, Game.Player.Character.GetOffsetPosition(new Vector3(0, 5.0f, 0)));
                car.EngineTorqueMultiplier = 100;
            }

            if (e.KeyCode == Keys.NumPad0) {
                Function.Call(Hash.SET_CLOCK_TIME, 21, 0, 0);
            }

            if (e.KeyCode == Keys.NumPad1) {
                Function.Call(Hash.SET_CLOCK_TIME, 7, 0, 0);
            }

            if (e.KeyCode == Keys.NumPad9)
            {
                Function.Call(Hash.CLEAR_PLAYER_WANTED_LEVEL, Game.Player);
            }
        }

        private float RandomFloat(float min, float max)
        {
            return (float)(rnd.NextDouble() * (max - min) + min);
        }

        private void HandleCreepers(int time)
        {
            if (time > 20 || time < 6)
            {
                if(creeperCounter < 10)
                {
                    float randomX = RandomFloat(5, 25);
                    float randomY = RandomFloat(5, 25);

                    GTA.UI.Notification.PostTicker("Spawning " + creeperCounter + ". Creeper", false);

                    int plusMinus = (int)RandomFloat(0, 1); // 0 = + ; 1 = - 
                    if (plusMinus == 0)
                    {
                        Creeper = World.CreatePed(PedHash.JayNorris, Game.Player.Character.GetOffsetPosition(new Vector3(randomX, randomY, 0)));
                        Creeper.Task.RunTo(Game.Player.Character.Position);
                        creeperCounter++;
                        SpawnedCreepers.Add(Creeper);
                    }
                    else if (plusMinus == 1)
                    {
                        Creeper = World.CreatePed(PedHash.JayNorris, Game.Player.Character.GetOffsetPosition(new Vector3(-randomX, -randomY, 0)));
                        Creeper.Task.RunTo(Game.Player.Character.Position);
                        creeperCounter++;
                        SpawnedCreepers.Add(Creeper);
                    }
                    else
                    {
                        GTA.UI.Notification.PostTicker("plusminus returned wrong number: " + plusMinus, false);
                    }  
                }
            }
            else if (time > 5 && time < 21)
            {
                for (int i = SpawnedCreepers.Count - 1; i >= 0; i--)
                {
                    Ped c = SpawnedCreepers[i];
                    if (c == null || !c.Exists())
                    {
                        SpawnedCreepers.RemoveAt(i);
                        creeperCounter--;
                        continue;
                    }
                    c.Delete();
                    creeperCounter--;
                }
            }
        }
        private void CreeperChasePlayer()
        {
            Vector3 currentPlayerPos = Game.Player.Character.Position;
            if(lastPlayerPosition != currentPlayerPos)
            {
                for(int i = SpawnedCreepers.Count - 1; i >= 0; i--)
                {
                    Ped c = SpawnedCreepers[i];
                    if (c == null || !c.Exists())
                    {
                        SpawnedCreepers.RemoveAt(i);
                        creeperCounter--;
                        continue;
                    }
                    bool creeperDead = Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, c, 1);
                    if (creeperDead)
                    {
                        c.Delete();
                        SpawnedCreepers.RemoveAt(i);
                        creeperCounter--;
                        continue;
                    }

                    if(c.Position.DistanceTo(currentPlayerPos) < 6)
                    {
                       Function.Call(Hash.ADD_EXPLOSION, c.Position.X, c.Position.Y, c.Position.Z, 7, 0.5, true, false, 100, false);
                    }
                    c.Task.RunTo(currentPlayerPos);
                }
                lastPlayerPosition = currentPlayerPos;
            }
            
        }
        private void HandleSkeletons(int time)
        {
            if (time > 20 || time < 6)
            {
                if (skeletonCounter < 5)
                {
                    float randomX = RandomFloat(20, 40);
                    float randomY = RandomFloat(20, 40);

                    GTA.UI.Notification.PostTicker("Spawning " + skeletonCounter + ". Skeleton", false);

                    int plusMinus = (int)RandomFloat(0, 1); // 0 = + ; 1 = - 
                    if (plusMinus == 0)
                    {
                        Skeleton = World.CreatePed(PedHash.Jesus01, Game.Player.Character.GetOffsetPosition(new Vector3(randomX, randomY, 0)));
                        Skeleton.Weapons.Give(WeaponHash.Firework, 5000, true, true);
                        Skeleton.Task.ShootAt(Game.Player.Character);
                    }
                    else if (plusMinus == 1)
                    {
                        Skeleton = World.CreatePed(PedHash.Jesus01, Game.Player.Character.GetOffsetPosition(new Vector3(-randomX, -randomY, 0)));
                        Skeleton.Weapons.Give(WeaponHash.Firework, 5000, true, true);
                        Skeleton.Task.ShootAt(Game.Player.Character);
                    }
                    else
                    {
                        GTA.UI.Notification.PostTicker("plusminus returned wrong number: " + plusMinus, false);
                    }
                }
            }
            else if (time > 5 && time < 21)
            {
                for (int i = SpawnedSkeletons.Count - 1; i >= 0; i--)
                {
                    Ped c = SpawnedSkeletons[i];
                    if (c == null || !c.Exists())
                    {
                        SpawnedSkeletons.RemoveAt(i);
                        skeletonCounter--;
                        continue;
                    }
                    c.Delete();
                    skeletonCounter--;
                }
            }
        }
        private void SkeletonShootPlayer()
        {
            for (int i = SpawnedSkeletons.Count - 1; i >= 0; i--)
            {
                Ped s = SpawnedSkeletons[i];
                if (s == null || !s.Exists())
                {
                    SpawnedSkeletons.RemoveAt(i);
                    skeletonCounter--;
                    continue;
                }
                bool skeletonDead = Function.Call<bool>(Hash.IS_PED_DEAD_OR_DYING, s, 1);
                if (skeletonDead)
                {
                    s.Delete();
                    SpawnedSkeletons.RemoveAt(i);
                    skeletonCounter--;
                    continue;
                }

                s.Task.ShootAt(Game.Player.Character);
            }
        }
    }
}