﻿using Common.UI.Interfaces;
using ComputerMonitoringTests.Common.UI.Tests.Interfaces.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ComputerMonitoringTests.Common.UI.Tests.Interfaces.Fixtures
{
    public class DialogViewFixture : IDialog
    {
        private const int MAX_TIME_MILLISECONDS = 3000;

        private object _dataContext;
        private Stopwatch timer;

        public DialogViewFixture()
        {
            DialogResult = null;
        }

        public object DataContext 
        {
            get { return _dataContext; }
            set { _dataContext = value; }
        }

        private bool? _dialogResult;
        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { _dialogResult = value; }
        }

        private Window _owner;
        public Window Owner 
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public void Close()
        {
            throw new DialogServiceClosedException();
        }

        public bool? ShowDialog()
        {
            timer = new Stopwatch();
            timer.Start();
            while (timer.ElapsedMilliseconds <= MAX_TIME_MILLISECONDS && DialogResult == null) { }
            return DialogResult;
        }
    }
}
