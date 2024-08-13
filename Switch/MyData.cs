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
        private bool _isLoadProcessAnim;
        public bool isLoadProcessAnim
        {
            get { return _isLoadProcessAnim; }
            set
            {
                _isLoadProcessAnim = value;
                OnPropertyChanged(nameof(isLoadProcessAnim));

            }
        }


        //Для настроек
        private int _searchDistancePL = UserData.searchDistancePL;
        private string _defaultBlock = UserData.defaultBlock;
        private int _roundCoordinateDistFileExcel = UserData.roundCoordinateDistFileExcel;
        private int _roundCoordinateXYFileExcel = UserData.roundCoordinateXYFileExcel;
        private bool _isDrawZoneSearchPL = UserData.isDrawZoneSearchPL;
        private double _searchLengthPL = UserData.searchLengthPL;
        private bool _isSelectSearchPL = UserData.isSelectSearchPL;




        //Для основной палитры
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
        // Тут переменные 
        #region Variable


        public int searchDistancePL
        {
            get { return _searchDistancePL; }
            set
            {
                _searchDistancePL = value; OnPropertyChanged(nameof(searchDistancePL));
            }
        }
        public string defaultBlock
        {
            get { return _defaultBlock; }
            set
            {
                _defaultBlock = value;
                OnPropertyChanged(nameof(defaultBlock));
            }
        }
        public int roundCoordinateDistFileExcel
        {
            get { return _roundCoordinateDistFileExcel; }
            set
            {
                _roundCoordinateDistFileExcel = value; OnPropertyChanged(nameof(roundCoordinateDistFileExcel));
            }
        }
        public int roundCoordinateXYFileExcel
        {
            get { return _roundCoordinateXYFileExcel; }
            set
            {
                _roundCoordinateXYFileExcel = value; OnPropertyChanged(nameof(roundCoordinateXYFileExcel));
            }
        }
        public bool isDrawZoneSearchPL
        {
            get { return _isDrawZoneSearchPL; }
            set
            {
                _isDrawZoneSearchPL = value;
                OnPropertyChanged(nameof(isDrawZoneSearchPL));
            }
        }

        public double searchLengthPL
        {
            get { return _searchLengthPL; }
            set
            {
                _searchLengthPL = value; OnPropertyChanged(nameof(searchLengthPL));
            }
        }

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


             public bool isSelectSearchPL
        {
            get { return _isSelectSearchPL; }
            set
            {
                _isSelectSearchPL = value;
                OnPropertyChanged(nameof(isSelectSearchPL));
            }
        }
        #endregion Variable

        public MyData(Tools tools)
        {
            _tools = tools;
            _tools.PropertyChanged += tools_PropertyChanged;
            UserData.StaticPropertyChanged += userData_PropertyChangedStatick;

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
            version = Version.ToString();




        }

        //Уведомление из UserData
        private void userData_PropertyChangedStatick(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UserData.searchDistancePL))
            {
                searchDistancePL = UserData.searchDistancePL;
                defaultBlock = UserData.defaultBlock;
                roundCoordinateDistFileExcel = UserData.roundCoordinateDistFileExcel;
                roundCoordinateXYFileExcel = UserData.roundCoordinateXYFileExcel;
                searchLengthPL = UserData.searchLengthPL;
                isDrawZoneSearchPL = UserData.isDrawZoneSearchPL;
                isSelectSearchPL = UserData.isSelectSearchPL;
            }
        }

        private void tools_PropertyChanged(object sender, PropertyChangedEventArgs e)
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
