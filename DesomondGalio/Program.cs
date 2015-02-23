﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;

namespace DesomondGalio
{
    internal class Program
    {
        public static Menu Menu;
        private static Obj_AI_Hero Player;
       

        public static Orbwalking.Orbwalker Orbwalker;

        public static Spell Q;
        public static Spell W;
        public static Spell E;
        public static Spell R;

        public static void Main(string[] args)
        {
            Game.OnGameStart += Game_Start;
            if (Game.Mode == GameMode.Running)
            {
                Game_Start(new EventArgs());
            }
        }
       
        public static void Game_Start(EventArgs args)
        {
            Menu = new Menu("Galio", "Galio", true);
            var TargetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(TargetSelectorMenu);
            Menu.AddSubMenu(TargetSelectorMenu);
            

            Menu.AddSubMenu(new Menu("Orbwalking", "Orbwalking"));
            Orbwalker = new Orbwalking.Orbwalker(Menu.SubMenu("Orbwalking"));


            //------------Combo
            Menu.AddSubMenu(new Menu("Combo", "Combo"));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseQ", "Use Q").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseW", "Use W").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseE", "Use E").SetValue(true));
            Menu.SubMenu("Combo").AddItem(new MenuItem("UseR", "Use R")).SetValue(true);
            Menu.SubMenu("Combo").AddItem(new MenuItem("MinEnemys", "Min enemys for R")).SetValue(new Slider(3, 5, 1));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboActive", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //-------------end Combo


            Menu.AddSubMenu(new Menu("Harass", "Harass"));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassQ", "Use Q").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassW", "Use W").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassE", "Use E").SetValue(false));
            Menu.SubMenu("Harass").AddItem(new MenuItem("HarassActive", "Harass!").SetValue(new KeyBind("20".ToCharArray()[0], KeyBindType.Press)));

           
            Menu.AddSubMenu(new Menu("Clear", "Clear"));
            Menu.SubMenu("Clear").AddItem(new MenuItem("UseQFarm", "Use Q").SetValue(true));
            Menu.SubMenu("Clear").AddItem(new MenuItem("UseEFarm", "Use E").SetValue(true));
            Menu.SubMenu("Clear").AddItem(new MenuItem("ClearActive", "Clear!").SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));


            var mana = Menu.AddSubMenu(new Menu("Mana Limiter", "Mana Limiter"));
            mana.AddItem(new MenuItem("comboMana", "Combo Mana %").SetValue(new Slider(1, 100, 0)));
            mana.AddItem(new MenuItem("harassMana", "Harass Mana %").SetValue(new Slider(30, 100, 0)));

 
            Menu.AddToMainMenu();

          

            Player = ObjectManager.Player;

            Q = new Spell(SpellSlot.Q, 940f);
            W = new Spell(SpellSlot.W, 800f);
            E = new Spell(SpellSlot.E, 1180f);
            R = new Spell(SpellSlot.R, 560f);

            Q.SetSkillshot(0.5f, 120, 1300, false, SkillshotType.SkillshotCircle);
            E.SetSkillshot(0.5f, 140, 1200, false, SkillshotType.SkillshotLine);
            R.SetSkillshot(0.5f, 300, 0, false, SkillshotType.SkillshotCircle);

            Game.PrintChat("DesomondGalio Loaded.");
            Game.OnGameUpdate += Game_OnUpdate;
            Drawing.OnDraw += OnDraw;
        }

        public static void Game_OnUpdate(EventArgs args)
        {
            List<Vector2> pos = new List<Vector2>();

            var ClearActive = Menu.Item("ClearActive").GetValue<KeyBind>().Active;
            var HarassActive = Menu.Item("HarassActive").GetValue<KeyBind>().Active;
            var ComboActive = Menu.Item("ComboActive").GetValue<KeyBind>().Active;
            var harassMana = Menu.Item("harassMana").GetValue<Slider>().Value;


            if (ClearActive)
            {
                Farm();
            }
            if (HarassActive && harassMana < (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana))
            {
                Harass();
            }
            if (ComboActive)
            {
                Combo();
            }
        }

        public static void Farm()
        {
            List<Vector2> pos = new List<Vector2>();
            bool qFarm = Menu.Item("UseQFarm").GetValue<bool>();
            bool eFarm = Menu.Item("UseEFarm").GetValue<bool>();
            
            var AllMinions = MinionManager.GetMinions(Player.ServerPosition, Q.Range, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.Health);
            foreach (var minion in AllMinions)
            {
                if (qFarm && Q.IsReady())
                {
                    Q.Cast(minion);
                }
                if (eFarm && E.IsReady())
                {
                    E.Cast(minion);
                }
            } 
        }

        public static void Harass()
        {
            var HarassQ = Menu.Item("HarassQ").GetValue<bool>();
            var HarassW = Menu.Item("HarassW").GetValue<bool>();
            var HarassE = Menu.Item("HarassE").GetValue<bool>();
            var t = TargetSelector.GetTarget(940, TargetSelector.DamageType.Magical);
           
            if (HarassQ && Q.IsReady())
            {
                
                if (t.IsValidTarget())
                {
                    if (HarassW && W.IsReady())
                    {
                        W.Cast(ObjectManager.Player);
                        Q.Cast(t);   
                    }
                    else
                    {
                        Q.Cast(t);
                    }
                }
            }

            if (HarassE && E.IsReady())
            {
                if (t.IsValidTarget())
                {
                    if (HarassW && W.IsReady())
                    {

                        W.Cast(ObjectManager.Player);
                        E.Cast(t);

                    }
                    else
                    {
                        E.Cast(t);
                    }
                }
            }
        }

        public static void Combo()
        {

            var comboMana = Menu.Item("comboMana").GetValue<Slider>().Value;
            var useR = Menu.Item("UseR").GetValue<bool>();
            var useQ = Menu.Item("UseQ").GetValue<bool>();
            var useW = Menu.Item("UseW").GetValue<bool>();
            var useE = Menu.Item("UseE").GetValue<bool>();
            var numOfEnemies = Menu.Item("MinEnemys").GetValue<Slider>().Value;
            var t = TargetSelector.GetTarget(940, TargetSelector.DamageType.Magical);
  
            if (!Player.HasBuff("GalioIdolOfDurand"))
            {
                Orbwalker.SetMovement(true);
            }

            if (useQ && Q.IsReady())
            {
                if (t.IsValidTarget())
                {
                    if (useW && W.IsReady() && comboMana < (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana))
                    {
                            W.Cast(ObjectManager.Player);
                            Q.Cast(t);
                    }
                    else
                    {
                        Q.Cast(t);
                    }
                }
            }

            if (useE && E.IsReady())
            {
                if (t.IsValidTarget())
                {
                    if (useW && W.IsReady() && comboMana < (ObjectManager.Player.Mana / ObjectManager.Player.MaxMana))
                    {
                        W.Cast(ObjectManager.Player);
                        E.Cast(t);
                    }
                    else
                    {
                        E.Cast(t);
                    }
                }
            }

            if (R.IsReady() && useR)
            {
                var t2 = TargetSelector.GetTarget(300, TargetSelector.DamageType.Magical);
                if(GetEnemys(t2) >= numOfEnemies)
                {
                    Orbwalker.SetMovement(false);
                    R.Cast(t2, false, true);
                    if (useW)
                    {
                        W.Cast(ObjectManager.Player);
                    }
                }
               
            }
        }
     //-----------Stolen
        private static int GetEnemys(Obj_AI_Hero target)
        {
            int Enemys = 0;
            foreach (Obj_AI_Hero enemys in ObjectManager.Get<Obj_AI_Hero>())
            {
                var pred = R.GetPrediction(enemys, true);
                if (pred.Hitchance >= HitChance.High && !enemys.IsMe && enemys.IsEnemy && Vector3.Distance(Player.Position, pred.UnitPosition) <= R.Range)
                {
                    Enemys = Enemys + 1;
                }
            }
            return Enemys;
        }
     //-----------Stolen
        private static void OnDraw(EventArgs args)
        {
             Utility.DrawCircle(ObjectManager.Player.Position, Q.Range, System.Drawing.Color.White, 1, 100);
             Utility.DrawCircle(ObjectManager.Player.Position, E.Range, System.Drawing.Color.White, 1, 100);
             Utility.DrawCircle(ObjectManager.Player.Position, R.Range, System.Drawing.Color.White, 1, 100);
        }
    }
}