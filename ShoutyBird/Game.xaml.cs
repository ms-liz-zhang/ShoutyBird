﻿using System.Windows.Controls;
using ShoutyBird.ViewModels;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : UserControl
    {
        public Game()
        {
            InitializeComponent();
            Display.Focus();
        }
    }
}
