/*
 * Created by SharpDevelop.
 * User: jose.avila
 * Date: 1/17/2018
 * Time: 6:09 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace CodeBuilder
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class Window1 : Window
	{
		readonly string EXTENSION_RESPALDO = ".bak";
//		static readonly ILog log4net =
//			LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		readonly System.Windows.Forms.FolderBrowserDialog folderBrowser;
		readonly System.Windows.Forms.OpenFileDialog openFile;
		
		public Window1()
		{
			InitializeComponent();
			
			folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
			openFile = new System.Windows.Forms.OpenFileDialog();
		}
		
		#region Funciones creadas
		void Log(string mensaje) {
			File.AppendAllText("HelpBuilder.log", mensaje + "\n");
		}
		
		string GenerarBeforePost(string tabla) {
			string _tabla;
			string str;
			
			_tabla = tabla.Trim();
			str =
				"  switch (TABLAQry->State) {\n" +
				"    case dsInsert:\n" +
				"      TABLASQL->Apply(ukInsert);\n" +
				"      break;\n" +
				"    case dsEdit:\n" +
				"      TABLASQL->Apply(ukModify);\n" +
				"      break;\n" +
				"  }\n";
			
			return str.Replace("TABLA", _tabla);
		}
		
		string GenerarBeforeDelete(string tabla) {
			string _tabla;
			string str;
			
			_tabla = tabla.Trim();
			str = "  TABLASQL->Apply(ukDelete);";
			
			return str.Replace("TABLA", _tabla);
		}
		
		string GenerarGridOnDrawColumnCell(string tabla, string campo) {
			string _tabla;
			string _campo;
			string str;
			
			_tabla = tabla.Trim();
			_campo = campo.Trim();
			
			str =
				"  if (Column->Field == TABLAQryCAMPO) {\n" +
				"    if (TABLAQryCAMPO->AsString == \"T\") {\n" +
				"      TABLAGrd->Canvas->Brush->Color = clGreen;\n" +
				"    } else {\n" +
				"      TABLAGrd->Canvas->Brush->Color = clRed;\n" +
				"    }\n" +
				"\n" +
				"    TABLAGrd->Canvas->Font->Color = clWhite;\n" +
				"    TABLAGrd->DefaultDrawColumnCell(Rect, DataCol, Column, State);\n" +
				"  }";
			
			return str.Replace("TABLA", _tabla).Replace("CAMPO", _campo);
		}
		
		string GenerarGridOnDblClick(string tabla, string campo) {
			string _tabla;
			string _campo;
			string str;
			
			_tabla = tabla.Trim();
			_campo = campo.Trim();
			
			str =
				"  if (TABLAGrd->SelectedField == TABLAQryCAMPO) {\n" +
				"    TABLAQry->Edit();\n" +
				"\n" +
				"    if (TABLAQryCAMPO->AsString == \"T\") {\n" +
				"      TABLAQryCAMPO->Value = \"F\";\n" +
				"    } else {\n" +
				"      TABLAQryCAMPO->Value = \"T\";\n" +
				"    }\n" +
				"  }";
			
			return str.Replace("TABLA", _tabla).Replace("CAMPO", _campo);
		}
		
		string GenerarMenuFormClick(string forma) {
			string _forma;
			string str;
			
			_forma = forma.Trim();
			
			str =
				"  if(!M->EsElSupervisor)\n" +
				"    return;\n" +
				"\n" +
				"  if(FORMA && !FORMA->Visible) {\n" +
				"    delete FORMA;\n" +
				"    FORMA = NULL;\n" +
				"  }\n" +
				"  if(!FORMA) {\n" +
				"    FORMA = new TFORMA(MainForm);\n" +
				"    if(!FORMA) return;\n" +
				"  }\n" +
				"\n" +
				"  FORMA->Show();\n" +
				"  FORMA->WindowState = wsNormal;";
			
			return str.Replace("FORMA", _forma);
		}
		
		string GenerarVistazo(string forma) {
			string _forma = forma.Trim();
			string str;
			
			str =
				"  FORMA = new TFORMA(this);\n" +
				"\n" +
				"  try {\n" +
				"    if(FORMA->ShowModal() == mrOk) {\n" +
				"      // Crear codigo\n" +
				"    }\n" +
				"\n" +
				"  } __finally {\n" +
				"    delete FORMA;\n" +
				"  }";
			
			return str.Replace("FORMA", _forma);
		}
		
		List<DatosArchivo> EncontrarArchivosTodo() {
			List<string> LInclusion;
			var LAparicion = new List<DatosArchivo>();
			var HSArchivo = new HashSet<string>();
			
			LInclusion = new List<string>(TxtInclusiones.Text.Split(' '));
			
			foreach(string inclusion in LInclusion) {
				var files = Directory.EnumerateFiles(TxtRuta.Text, "*.*", SearchOption.TopDirectoryOnly)
					.Where(file => file.EndsWith(inclusion, true, System.Globalization.CultureInfo.CurrentCulture)
					       && !file.EndsWith(EXTENSION_RESPALDO, true, System.Globalization.CultureInfo.CurrentCulture));
				
				HSArchivo.UnionWith(files);
			}
			
			foreach(string archivo in HSArchivo) {
				string excluido;
				string[] contenido;
				int conteoContenido;
				bool debeAgregarse;
				bool debeEsperar;
				bool lineaInicia;
				bool lineaTermina;
				int inicio;
				int final;
				
				excluido = TxtExclusiones.Text.Trim();
				contenido = File.ReadAllLines(archivo);
				conteoContenido = 0;
				inicio = 0;
				final = 0;
				debeAgregarse = false;
				debeEsperar = false;
				
				foreach(string linea in contenido) {
					string _linea;
					
					_linea = linea.Trim();
					conteoContenido++;
					
					lineaInicia = _linea.StartsWith("/* TODO -o");
					lineaTermina = _linea.EndsWith("*/");

					if (lineaInicia) {
						inicio = conteoContenido;
						
						if (lineaTermina) {
							debeAgregarse = true;
							debeEsperar = false;
						} else {
							debeEsperar = true;
							debeAgregarse = false;
						}
					}
					
					if (debeEsperar && lineaTermina) {
						final = conteoContenido;
						debeEsperar = false;
						debeAgregarse = true;
					}
					
					if (debeAgregarse) {
						if (!string.IsNullOrEmpty(excluido)) {
							if (_linea.IndexOf(excluido, StringComparison.OrdinalIgnoreCase) >= 0) {
								debeAgregarse = false;
							}
						}
					}
					
					if (debeAgregarse) {
						final = conteoContenido;
						
						DatosArchivo item = new DatosArchivo();
						item.Archivo = archivo;
						item.Contenido = _linea;
						item.Inicio = inicio;
						item.Final = final;
						
						LAparicion.Add(item);
						
						debeAgregarse = false;
						debeEsperar = false;
					}
				}
			}
			
			return LAparicion;
		}
		
		void CrearArchivoRespaldo(string archivo) {
			string archivoRespaldo;
			archivoRespaldo = archivo + EXTENSION_RESPALDO;

			if (!File.Exists(archivoRespaldo)) {
				File.Copy(archivo, archivoRespaldo);
			}
		}
		
		void EliminarContenidoTodo() {
			string[] contenido;
			string archivo;

			archivo = string.Empty;
			contenido = null;
			
			foreach(var item in LstApariciones.Items) {
				DatosArchivo entidad = item as DatosArchivo;
				
				if (!entidad.Archivo.Equals(archivo)) {
					if (!string.IsNullOrEmpty(archivo)) {
						if ((bool)ChkRespaldar.IsChecked) {
							CrearArchivoRespaldo(entidad.Archivo);
						}
						
						File.WriteAllLines(entidad.Archivo, contenido);
					}
					
					archivo = entidad.Archivo;
					contenido = File.ReadAllLines(archivo);
				}
				
				for (int i = entidad.Inicio; i <= entidad.Final; i++) {
					contenido[i-1] = string.Empty;
				}
			}

			if ((bool)(ChkRespaldar.IsChecked)) {
				CrearArchivoRespaldo(archivo);
			}

			File.WriteAllLines(archivo, contenido);
		}
		
		void RestaurarArchivos() {
			string archivoOriginal;
			
			var archivos =
				Directory.EnumerateFiles(TxtRuta.Text, "*" + EXTENSION_RESPALDO, SearchOption.TopDirectoryOnly);
			
			foreach(string archivo in archivos) {
				archivoOriginal = archivo.Replace(EXTENSION_RESPALDO, string.Empty);
				File.Delete(archivoOriginal);
				File.Move(archivo, archivoOriginal);
			}
		}
		#endregion
		
		void BtnGrid_Click(object sender, RoutedEventArgs e)
		{
			if (CmbEventoGrid.Text.Equals("Draw Column")) {
				MmoResultadoGrid.Text = GenerarGridOnDrawColumnCell(TxtTabla.Text, TxtCampo.Text);
			} else if (CmbEventoGrid.Text.Equals("Double Click")){
				MmoResultadoGrid.Text = GenerarGridOnDblClick(TxtTabla.Text, TxtCampo.Text);
			}
		}
		
		void BtnGenerarForma_Click(object sender, RoutedEventArgs e)
		{
			if (CmbEventosForma.Text.Equals("Menu")) {
				MmoResultadoMenu.Text = GenerarMenuFormClick(TxtForma.Text);
			} else if (CmbEventosForma.Text.Equals("Vistazo")) {
				MmoResultadoMenu.Text = GenerarVistazo(TxtForma.Text);
			}
		}
		
		void BtnGenerarTQuery_Click(object sender, RoutedEventArgs e)
		{
			if (CmbEventosTQuery.Text.Equals("Before Post")) {
				MmoResultadoTQuery.Text = GenerarBeforePost(TxtTabla2.Text);
			} else if (CmbEventosTQuery.Text.Equals("Before Delete")) {
				MmoResultadoTQuery.Text = GenerarBeforeDelete(TxtTabla2.Text);
			}
		}
		
		void BtnAbrirDirectorio_Click(object sender, RoutedEventArgs e)
		{
			if (folderBrowser.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK)) {
				TxtRuta.Text = folderBrowser.SelectedPath;
			}
		}
		
		void BtnBuscarTodo_Click(object sender, RoutedEventArgs e)
		{
			LstApariciones.ItemsSource = null;
			if (!TxtRuta.Text.Equals(string.Empty)) {
				LstApariciones.ItemsSource = EncontrarArchivosTodo();
			}
		}
		
		void BtnEliminarTodo_Click(object sender, RoutedEventArgs e)
		{
			EliminarContenidoTodo();
		}
		
		void BtnRestaurarTodo_Click(object sender, RoutedEventArgs e)
		{
			RestaurarArchivos();
		}
		
		void BtnAbrirDirPlantilla_Click(object sender, RoutedEventArgs e)
		{			
			if (openFile.ShowDialog().Equals(System.Windows.Forms.DialogResult.OK)) {
				TxtRutaPlantilla.Text = openFile.FileName;
			}
		}
		
		void BtnPlantilla_Click(object sender, RoutedEventArgs e)
		{
			string contenido = File.ReadAllText(TxtRutaPlantilla.Text);
			contenido = contenido.Replace("{ENLACE}", TxtEnlace.Text);
			contenido = contenido.Replace("{IMAGEN}", TxtImg.Text);
			
			TxtCorreoPlantilla.Text = contenido;
		}
		
		void BtnCopiarPlantilla_Click(object sender, RoutedEventArgs e)
		{
			Clipboard.SetDataObject(TxtCorreoPlantilla.Text);
		}
		
		void BtnLimpiarPlantilla_Click(object sender, RoutedEventArgs e)
		{
			TxtCorreoPlantilla.Text = string.Empty;
			TxtEnlace.Text = string.Empty;
			TxtImg.Text = string.Empty;
		}
		
		void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}