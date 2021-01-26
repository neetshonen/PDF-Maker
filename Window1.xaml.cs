using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using PdfSharp;
using MigraDoc;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Diagnostics;
using Microsoft.Win32;

namespace FTPuploader
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public static Window MainForm;
        public Window1(Window mainform)
        {
            this.Top = mainform.Top + 275;
            this.Left = mainform.Left + mainform.Width+10;
            InitializeComponent();
            MainForm = mainform;
            ddhelper.Content= "Drag and Drop files here" + Environment.NewLine + "to make PDF file...";
        }
        public Window1()
        {
            InitializeComponent();
            ddhelper.Content = "Drag and Drop files here" + Environment.NewLine + "to make PDF file...";
        }

        private void ListView_Drop(object sender, DragEventArgs e)
        {

            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[]; // get all files dropped
            if (files != null && files.Any())
                foreach (var f in files)
                {
                    try
                    {
                        if (File.Exists(f) && !pdflist.Items.Contains(f)&&(f.Contains("png")|| f.Contains("jpg")|| f.Contains("jpeg")|| f.Contains("pdf")|| f.Contains("PNG") || f.Contains("JPG") || f.Contains("JPEG") || f.Contains("PDF")))
                        {
                            pdflist.Items.Add(f + "");
                            ddhelper.Visibility = Visibility.Hidden;
                        }
                    }
                    catch
                    {
                    }

                }
        }

        private void Button_Click(object sender, RoutedEventArgs e)//save
        {
            
            makePDF("Preview");
            uploadbutton.IsEnabled = false;
            previewbutton.IsEnabled = false;
            pdflist.IsEnabled = false;

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)//upload
        {
            uploadbutton.IsEnabled = false;
            previewbutton.IsEnabled = false;
            pdflist.IsEnabled = false;
           makePDF("Upload");

        }
        private  void makePDF(String func)
        {
             Task.Run(() => {
                 Document document = new Document();

                 File.Delete("temp.pdf");


                 foreach (string s in pdflist.Items) {
                     string extension = s.Split('\\')[s.Split('\\').Count() - 1].Split('.')[s.Split('\\')[s.Split('\\').Count() - 1].Split('.').Count()-1];
                     //an einai eikona
                     if (extension == "png" || extension == "jpg" || extension == "jpeg"|| extension == "PNG" || extension == "JPG" || extension == "JPEG") {
                         MigraDoc.DocumentObjectModel.Section section = document.AddSection();
                         MigraDoc.DocumentObjectModel.Shapes.Image i = section.AddImage(s);
                         i.Width = "18cm";
                         i.LockAspectRatio = true;
                     }
                 }
                 //edw prepei na swsoume to document se pdf temp
                 PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true);
                 FileInfo fi = new FileInfo("temp.pdf");
                 fi.Attributes.HasFlag(FileAttributes.Hidden);
                 StreamWriter sw = fi.CreateText();
                 sw.Close();
                 if (document.Sections.Count == 0) {
                     document.AddSection().AddParagraph("");
                 }
                 pdfRenderer.Document = document;
                 pdfRenderer.RenderDocument();

                 pdfRenderer.PdfDocument.Save("temp.pdf");


                 //kai twra na enwsoume ta alla pdf me afto
                 bool combined = false;
                 PdfDocument output = PdfReader.Open("temp.pdf", PdfDocumentOpenMode.Import);
                 foreach (string s in pdflist.Items)
                 {
                     string extension = s.Split('\\')[s.Split('\\').Count() - 1].Split('.')[s.Split('\\')[s.Split('\\').Count() - 1].Split('.').Count() - 1];
                     //an einai eikona
                     if (extension == "pdf")
                     {


                         PdfDocument inputDocument1 = null ;
                         try
                         {
                             inputDocument1= PdfReader.Open(s, PdfDocumentOpenMode.Import);
                         }
                         catch(Exception ae) {
                             MessageBox.Show("File Corrupted","Error",MessageBoxButton.OK,MessageBoxImage.Error);
                             break;
                         }
                         int count = (inputDocument1.PageCount);

                         for (int idx = 0; idx < count; idx++)
                         {
                             PdfPage page = inputDocument1.Pages[idx];

                             // ...and add it to the output document.
                             output.AddPage(page);


                         }
                         combined = true;

                     }
                 }
                 if (combined == true)
                 {
                     output.Save("temp.pdf");

                 }
                 if (func == "Preview")
                 { 
                     Process.Start("temp.pdf");
                     this.Dispatcher.Invoke(() => {

                         uploadbutton.IsEnabled = true;
                         previewbutton.IsEnabled = true;
                         pdflist.Items.Clear();
                         pdflist.IsEnabled = true;
                     });
                 }
                else if (func == "Upload")
                {
                    SaveFileDialog savefiledialog = new SaveFileDialog();
                     savefiledialog.Filter = "PDF|*.pdf";
                     savefiledialog.Title = "Save PDF File";
                     if (savefiledialog.ShowDialog() == true)
                     {
                         if (File.Exists(savefiledialog.FileName))
                         {
                             File.Delete(savefiledialog.FileName);
                         }
                     if ((savefiledialog.FileName.Split('\\')[savefiledialog.FileName.Split('\\').Count() - 1].Split('.').Count() == 0))
                     {
                         savefiledialog.FileName += ".pdf";
                     }
                      
                         File.Copy("temp.pdf", savefiledialog.FileName);
                     }
                     this.Dispatcher.Invoke(() => {
                         pdflist.Items.Clear();
                         uploadbutton.IsEnabled = true;
                         previewbutton.IsEnabled = true;
                         pdflist.IsEnabled = true;

                     });
                    
                }
            });

        }
    }
}
