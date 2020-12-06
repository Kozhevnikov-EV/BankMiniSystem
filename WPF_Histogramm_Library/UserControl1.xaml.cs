﻿using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_Histogramm_Library
{
    /// <summary>
    /// Логика взаимодействия для Histogram.xaml
    /// </summary>
    public partial class Histogram : UserControl
    {

        public IEnumerable Data
        {
            get => list.ItemsSource;
            set => list.ItemsSource = value;
        }

        public Histogram()
        {
            InitializeComponent();
            list.ItemsSource = Data;
        }
    }
}
