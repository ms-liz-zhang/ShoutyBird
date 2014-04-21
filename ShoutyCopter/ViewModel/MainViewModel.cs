using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Timers;
using System.Windows.Input;
using System;

namespace ShoutyCopter.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private Vector _position;

        private const float Gravity = 9.8f;
        //Time between ticks in milliseconds
        private const double TimerTick = 10d;

        private const double JumpAcceleration = -19.6;

        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;

        private double _scaleFactor;
        private double _width;
        private double _height;

        public Vector DisplayPosition
        {
            get { return ToDisplayUnits(Position); }
        }

        public double DisplayHeight
        {
            get { return ToDisplayUnits(Height); }
        }

        public double DisplayWidth
        {
            get { return ToDisplayUnits(Width);  }
        }

        public double ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                if (_scaleFactor == value) return;
                _scaleFactor = value;
                RaisePropertyChanged("ScaleFactor");
            }
        }

        protected Vector Position
        {
            get { return _position; }
            set
            {
                if (Equals(_position, value)) return;
                _position = value;
                RaisePropertyChanged("DisplayPosition");
            }
        }

        public Vector Velocity
        {
            get { return _velocity; }
            set
            {
                if (Equals(_velocity, value)) return;
                _velocity = value;
                RaisePropertyChanged("Velocity");
            }
        }

        public Vector DeltaVelocity
        {
            get { return _deltaVelocity; }
            set
            {
                if (Equals(_deltaVelocity, value)) return;
                _deltaVelocity = value;
                RaisePropertyChanged("DeltaVelocity");
            }
        }

        public Vector DeltaPosition
        {
            get { return _deltaPosition; }
            set
            {
                if (Equals(_deltaPosition, value)) return;
                _deltaPosition = value;
                RaisePropertyChanged("DeltaPosition");
            }
        }

        protected Vector Acceleration { get; set; }

        protected double Width
        {
            get { return _width; }
            set
            {
                if (_width == value) return;
                _width = value;
                RaisePropertyChanged("DisplayWidth");
            }
        }

        protected double Height
        {
            get { return _height; }
            set
            {
                if (_height == value) return;
                _height = value;
                RaisePropertyChanged("DisplayHeight");
            }
        }

        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        public RelayCommand<KeyEventArgs> Move { get; private set; }
        public RelayCommand<MouseEventArgs> MouseCommand { get; private set; }

        public MainViewModel()
        {
            Position = new Vector { X = 0, Y = 0 };
            Width = 1;
            Height = 1;
            Timer worldTimer = new Timer(TimerTick) {AutoReset = true};
            worldTimer.Elapsed += Tick;
            worldTimer.Enabled = true;
            ScaleFactor = 10;
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);
            Acceleration = new Vector() {X = 0, Y = Gravity};
        }

        private bool _isBusy = false;
        private Vector _velocity;
        private Vector _deltaVelocity;
        private Vector _deltaPosition;

        /// <summary>
        /// Update the unit's acceleration, velocity, and position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object sender, ElapsedEventArgs e)
        {
            if (_isBusy) return;

            _isBusy = true;
            Timer timer = (Timer)sender;

            Vector prevVelocity = Velocity;

            Velocity = new Vector
            {
                X = CalculateVelocityChange(Acceleration.X, Velocity.X, timer.Interval),
                Y = CalculateVelocityChange(Acceleration.Y, Velocity.Y, timer.Interval)
            };

            DeltaVelocity = new Vector {X = Velocity.X - prevVelocity.X, Y = Velocity.Y - prevVelocity.Y};

            while (_actionQueue.Count > 0)
            {
                Action action = _actionQueue.Dequeue();

                if (action == Action.Jump)
                {
                    //Acceleration = new Vector {X = Acceleration.X, Y = JumpAcceleration};
                    Velocity = new Vector
                    {
                        X = Velocity.X,
                        Y = -10
                    };
                }
            }

            Vector prevPos = Position;
            Position = new Vector { X = Position.X, Y = CacluatePositionChange(Acceleration.Y, Velocity.Y, Position.Y, timer.Interval) };
            DeltaPosition = new Vector {X = Position.X - prevPos.X, Y = Position.Y - prevPos.Y};
            //Vertical acceleration is always set back to that of gravity after each tick of the simulation
            //Acceleration = new Vector() {X = Acceleration.X, Y = Gravity};
            _time += ((Timer)sender).Interval;
            _isBusy = false;
        }

        /// <summary>
        /// Calculates the position change
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="velocity"></param>
        /// <param name="position"></param>
        /// <param name="timeInterval">In milliseconds</param>
        /// <returns></returns>
        private double CacluatePositionChange(double acceleration, double velocity, double position, double timeInterval)
        {
            return acceleration * Math.Pow(timeInterval / 1000, 2) + velocity * timeInterval / 1000 + position;
        }

        private double CalculateVelocityChange(double acceleration, double velocity, double timeInterval)
        {
            return acceleration*timeInterval/1000 + velocity;
        }

        private void MoveExecute(KeyEventArgs keyEvent)
        {
            if (keyEvent.Key == Key.Space)
            {
                _actionQueue.Enqueue(Action.Jump);
            }
        }

        protected Vector ToDisplayUnits(Vector gameUnit)
        {
            return new Vector { X = gameUnit.X * ScaleFactor, Y = gameUnit.Y * ScaleFactor};
        }

        protected double ToDisplayUnits(double gameUnit)
        {
            return gameUnit*ScaleFactor;
        }
    }
}