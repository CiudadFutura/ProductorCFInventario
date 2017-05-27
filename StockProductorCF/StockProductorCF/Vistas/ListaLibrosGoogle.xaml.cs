﻿
using Google.GData.Client;
using Google.GData.Spreadsheets;
using Xamarin.Forms;
using StockProductorCF.Servicios;
using System.Collections.Generic;

namespace StockProductorCF.Vistas
{
	public partial class ListaLibrosGoogle
	{
		private readonly AtomEntryCollection _listaLibros;
		private readonly SpreadsheetsService _servicio;
		private bool _esTeclaPar;
		private double _anchoActual;

		public ListaLibrosGoogle(SpreadsheetsService servicio, AtomEntryCollection listaLibros)
		{
			InitializeComponent();
			Cabecera.Children.Add(App.ObtenerImagen(TipoImagen.EncabezadoProyectos));
			SombraEncabezado.Source = ImageSource.FromResource(App.RutaImagenSombraEncabezado);
			_servicio = servicio;
			_listaLibros = listaLibros;

			CargarListaLibros();
		}

		private void EnviarListaHojas(string linkLibro)
		{
			var hojas = new ServiciosGoogle().ObtenerListaHojas(linkLibro, _servicio);

			var paginaListaLibros = new ListaHojasCalculoGoogle(_servicio, hojas.Entries);
			Navigation.PushAsync(paginaListaLibros);
		}

		private void CargarListaLibros()
		{
			var listaLibros = new List<ClaseLibro>();
			foreach (var datosLibro in _listaLibros)
			{
				var libro = new ClaseLibro(datosLibro.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null).HRef.ToString(), datosLibro.Title.Text);
				listaLibros.Add(libro);
			}

			var vista = new ListView
			{
				RowHeight = 60,
				VerticalOptions = LayoutOptions.StartAndExpand,
				HorizontalOptions = LayoutOptions.Fill,
				ItemsSource = listaLibros,
				ItemTemplate = new DataTemplate(() =>
				{
					var nombreLibro = new Label
					{
						FontSize = 18,
						TextColor = Color.FromHex("#1D1D1B"),
						VerticalOptions = LayoutOptions.CenterAndExpand,
						HorizontalOptions = LayoutOptions.CenterAndExpand
					};
					nombreLibro.SetBinding(Label.TextProperty, "Nombre");
					
					var celda = new ViewCell
					{
						View = new StackLayout
						{
							Orientation = StackOrientation.Horizontal,
							Children = { nombreLibro }
						}
					};
					
					celda.Tapped += (sender, args) =>
					{
						EnviarListaHojas(((ClaseLibro)((ViewCell)sender).BindingContext).Link);
						celda.View.BackgroundColor = Color.Silver;
					};

					celda.Appearing += (sender, args) =>
					{
						var viewCell = (ViewCell)sender;
						if (viewCell.View != null)
						{
							viewCell.View.BackgroundColor = _esTeclaPar ? Color.FromHex("#EDEDED") : Color.FromHex("#E2E2E1");
						}
						_esTeclaPar = !_esTeclaPar;
					};

					return celda;
				})
			};

			ContenedorLibros.Children.Add(vista);
		}

		protected override async void OnSizeAllocated(double ancho, double alto)
		{
			base.OnSizeAllocated(ancho, alto);
			if (_anchoActual == ancho) return;
			if (ancho > alto)
			{
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, -100, 1000);
				GrupoEncabezado.IsVisible = false;
			}
			else
			{
				GrupoEncabezado.IsVisible = true;
				if (_anchoActual != 0)
					await GrupoEncabezado.TranslateTo(0, 0, 1000);
			}
			_anchoActual = ancho;
		}
	}

	//Clase Libro: utilizada para armar la lista scrolleable de libros
	[Android.Runtime.Preserve]
	public class ClaseLibro
	{
		[Android.Runtime.Preserve]
		public ClaseLibro(string link, string nombre)
		{
			Link = link;
			Nombre = nombre;
		}

		[Android.Runtime.Preserve]
		public string Link { get; }
		[Android.Runtime.Preserve]
		public string Nombre { get; }
	}
}
