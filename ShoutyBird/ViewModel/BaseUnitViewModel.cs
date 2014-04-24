﻿using System;
using System.Windows.Data;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ShoutyCopter;

namespace ShoutyBird.ViewModel
{
    public delegate void CollisionEventHandler(object sender, BaseUnitViewModel collidingUnit);
    public delegate void PositionChangedEventHandler(object sender, PositionChangedEventArgs e);

    public abstract class BaseUnitViewModel : ViewModelBase, IDisposable
    {
        private Vector _position;
        private double _scaleFactor;
        private double _width;
        private double _height;
        private Vector _velocity;
        private Brush _backgroundBrush;

        public event CollisionEventHandler Collision;
        public event PositionChangedEventHandler PositionChanged;
        public event EventHandler Updated;

        public Brush BackgroundBrush
        {
            get { return _backgroundBrush; }
            set
            {
                if (Equals(_backgroundBrush, value)) return;
                _backgroundBrush = value;
                RaisePropertyChanged("BackgroundBrush");
            }
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
            get { return ToDisplayUnits(Width); }
        }

        public Vertices Vertices { get; private set; }

        public Vector Position
        {
            get { return _position; }
            set
            {
                if (Equals(_position, value)) return;
                Vector prevPosition = _position;
                _position = value;
                UpdateVertices();
                RaisePropertyChanged("DisplayPosition");

                OnPositionChanged(prevPosition);
            }
        }

        /// <summary>
        /// In gameunits / second
        /// </summary>
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

        public Vector Acceleration { get; set; }

        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value) return;
                _width = value;
                UpdateVertices();
                RaisePropertyChanged("DisplayWidth");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (_height == value) return;
                _height = value;
                UpdateVertices();
                RaisePropertyChanged("DisplayHeight");
            }
        }

        public Vector ToDisplayUnits(Vector gameUnit)
        {
            return new Vector { X = gameUnit.X * ScaleFactor, Y = gameUnit.Y * ScaleFactor };
        }

        protected double ToDisplayUnits(double gameUnit)
        {
            return gameUnit * ScaleFactor;
        }

        protected BaseUnitViewModel()
        {
            Updated += (sender, args) => Messenger.Default.Send(new UnitUpdateMessage(this));
        }

        public virtual void Update(double timeInterval)
        {
            OnUpdated();

            Velocity = new Vector
            {
                X = CalculateVelocityChange(Acceleration.X, Velocity.X, timeInterval),
                Y = CalculateVelocityChange(Acceleration.Y, Velocity.Y, timeInterval)
            };

            Vector prevPosition = Position;
            Position = new Vector
            {
                X = CacluatePositionChange(Acceleration.X, Velocity.X, Position.X, timeInterval),
                Y = CacluatePositionChange(Acceleration.Y, Velocity.Y, Position.Y, timeInterval)
            };
        }

        /// <summary>
        /// Calculates the position change
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="velocity"></param>
        /// <param name="position"></param>
        /// <param name="timeInterval">In milliseconds</param>
        /// <returns></returns>
        protected double CacluatePositionChange(double acceleration, double velocity, double position, double timeInterval)
        {
            return acceleration * Math.Pow(timeInterval / 1000, 2) + velocity * timeInterval / 1000 + position;
        }

        protected double CalculateVelocityChange(double acceleration, double velocity, double timeInterval)
        {
            return acceleration * timeInterval / 1000 + velocity;
        }

        protected void OnCollision(object sender, BaseUnitViewModel collidingUnit)
        {
            if (Collision != null)
                Collision(sender, collidingUnit);
        }

        protected void OnUpdated()
        {
            if (Updated != null)
                Updated(this, null);
        }

        protected void OnPositionChanged(Vector prevPosition)
        {
            if (PositionChanged != null)
                PositionChanged(this, new PositionChangedEventArgs(prevPosition, Position));
        }

        private void UpdateVertices()
        {
            Vertices = new Vertices
                       {
                           X1 = Position.X,
                           X2 = Position.X + Width,
                           Y1 = Position.Y,
                           Y2 = Position.Y + Height
                       };
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
