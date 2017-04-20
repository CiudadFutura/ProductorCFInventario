﻿
using System;
using StockProductorCF.Clases;
using Xamarin.Forms;
using StockProductorCF.Vistas;

namespace StockProductorCF
{
	public partial class App : Application
	{
		public static int AnchoDePantalla;
		public App()
		{
			InitializeComponent();

			var columnasParaVer = CuentaUsuario.ObtenerColumnasParaVer();
			var columnasInventario = CuentaUsuario.ObtenerColumnasInventario();

			switch (CuentaUsuario.ObtenerAccesoDatos())
			{
				case "G":
					AccesoHojaDeCalculoGoogle(columnasParaVer, columnasInventario);
					break;
				case "B":
					AccesoBaseDeDatos(columnasParaVer, columnasInventario);
					break;
				default:
					MainPage = new NavigationPage(new AccesoDatos());
					break;
			}
		}

		private void AccesoHojaDeCalculoGoogle(string columnasParaVer, string columnasInventario)
		{
			var linkHojaConsulta = CuentaUsuario.ObtenerLinkHojaInventario();

			if (string.IsNullOrEmpty(linkHojaConsulta) || string.IsNullOrEmpty(columnasParaVer) || string.IsNullOrEmpty(columnasInventario) ||
				!CuentaUsuario.ValidarTokenDeGoogle())
					MainPage = new NavigationPage(new PaginaAuntenticacion(!string.IsNullOrEmpty(linkHojaConsulta) && !string.IsNullOrEmpty(columnasParaVer) && !string.IsNullOrEmpty(columnasInventario)));
				else
					MainPage = new NavigationPage(new PaginaGrilla(linkHojaConsulta, null));
		}

		private void AccesoBaseDeDatos(string columnasParaVer, string columnasInventario)
		{
			if (string.IsNullOrEmpty(columnasParaVer) || string.IsNullOrEmpty(columnasInventario))
				MainPage = new NavigationPage(new PaginaConexionBaseDeDatos());
			else
				MainPage = new NavigationPage(new PaginaGrilla());
		}

		[Android.Runtime.Preserve]
		public static void AlmacenarAnchoPantalla(int ancho)
		{
			AnchoDePantalla = ancho;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

	}
}
