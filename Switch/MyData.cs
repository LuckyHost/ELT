using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static ElectroTools.Tools;

namespace ElectroTools
{
    public class MyData : INotifyPropertyChanged
    {
        //Анимация
        private  bool _isLoadProcessAnim;
        public  bool isLoadProcessAnim
        {
            get { return _isLoadProcessAnim; }
            set
            {
                _isLoadProcessAnim = value;
                OnPropertyChanged(nameof(isLoadProcessAnim));

            }
        }

    


        private string _pathDLLFile;
        private string _pathDWGFile;
        private bool _isLock;
        private bool _isOpenTableVoltage;
        private bool _isOpenTableSS;
        private ObservableCollection<PointLine> _listpoint;
        private ObservableCollection<PointLine> _listLastPoint;
        private ObservableCollection<PowerLine> _listPowerLine;
        private ObservableCollection<Edge> _listEdge;
        public Tools _tools;
        //Версия модуля
        private string _version;

        public string version
        {
            get { return _version; }
            set
            {
                _version = value;
                OnPropertyChanged(nameof(version));

            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<PointLine> listpoint
        {
            get { return _listpoint; }
            set
            {
                _listpoint = value;
                OnPropertyChanged(nameof(listpoint));

            }
        }

        public ObservableCollection<PointLine> listLastPoint
        {
            get { return _listLastPoint; }
            set
            {
                _listLastPoint = value;
                OnPropertyChanged(nameof(listLastPoint));
            }
        }

        public ObservableCollection<PowerLine> listPowerLine
        {
            get { return _listPowerLine; }
            set
            {
                _listPowerLine = value;
                OnPropertyChanged(nameof(listPowerLine));
            }
        }

        public ObservableCollection<Edge> listEdge
        {
            get { return _listEdge; }
            set
            {
                _listEdge = value;
                OnPropertyChanged(nameof(listEdge));
            }
        }

        public string pathDLLFile
        {
            get { return _pathDLLFile; }
            set
            {
                _pathDLLFile = value;
                OnPropertyChanged(nameof(pathDLLFile));
            }
        }

        public bool isLock
        {
            get { return _isLock; }
            set
            {
                if (_isLock != value)
                {
                    _isLock = value;
                    OnPropertyChanged(nameof(isLock));
                }
            }
        }

  

        public bool isOpenTableSS
        {
            get { return _isOpenTableSS; }
            set
            {
                if (value != _isOpenTableSS)
                {
                    _isOpenTableSS = value;
                    OnPropertyChanged(nameof(isOpenTableSS));
                }
            }
        }


        public string pathDWGFile
        {
            get { return _pathDWGFile; }

            set
            {
                _pathDWGFile = value;
                OnPropertyChanged(nameof(pathDWGFile));
            }

        }


        public MyData(Tools tools)
        {
            _tools = tools;
            _tools.PropertyChanged += Tools_PropertyChanged;

            pathDLLFile = _tools.pathDLLFile;
            listpoint = new ObservableCollection<PointLine>(_tools.listPoint);
            listLastPoint = new ObservableCollection<PointLine>(_tools.listLastPoint);
            listPowerLine = new ObservableCollection<PowerLine>(_tools.listPowerLine);
            listEdge = new ObservableCollection<Edge>(_tools.listEdge);
            isLock = !true;
            isOpenTableSS = false;
            isLoadProcessAnim = true;

            Assembly assembly = Assembly.GetExecutingAssembly();
            Version Version = assembly.GetName().Version;
            version= Version.ToString();
        }



        private void Tools_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Tools.pathDLLFile))
            {
                pathDLLFile = _tools.pathDLLFile;
            }

            if (e.PropertyName == nameof(Tools.listPoint))
            {
                listpoint = new ObservableCollection<PointLine>(_tools.listPoint);

                if (listpoint != null)
                {
                    isLock = !false;
                }
            }

            if (e.PropertyName == nameof(Tools.listEdge))
            {
                listEdge = new ObservableCollection<Edge>(_tools.listEdge);
            }

            if (e.PropertyName == nameof(Tools.listPowerLine))
            {
                listPowerLine = new ObservableCollection<PowerLine>(_tools.listPowerLine);
            }



            //получаю имя файла
            if (e.PropertyName == nameof(MyOpenDocument.ed))
            {
                string[] tempArrayString = MyOpenDocument.ed.Document.Name.Split('\\');
                pathDWGFile = tempArrayString.Last();
            }

        }




        //Уведомление WPF
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
