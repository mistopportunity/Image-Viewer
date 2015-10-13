using System.IO;
using System.Collections.Generic;
using System.Windows;

namespace Image_Viewer {
    public partial class App:Application {
        private void Application_Startup(object sender,StartupEventArgs e) {
            if(e.Args.Length > 0) {
                List<string> files = new List<string>();
                foreach(string file in e.Args) {
                    files.Add(file);
                }
                if(files.Count > 0) {
                    MainWindow window = new MainWindow(files.ToArray());
                    window.Show();
                    return;
                }
            }
            nonParamaterizedStart();
        }
        private void nonParamaterizedStart() {
            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}