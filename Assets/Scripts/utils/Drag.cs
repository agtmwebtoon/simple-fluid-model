using System;
using DefaultNamespace.InfoClass;
using UnityEngine;

namespace DefaultNamespace.utils
{
    public class Drag
    {
        private double _skinFrictionDrag;
        private double _dragCoef;
        public double dragForce;
        
        private void _calcSkinFriction(double RayNum)
        {
            double _correctionFactor = 0;
            double _RayNumCrit = 51 * Math.Pow((Rocket.RocketRoughness / Rocket.RocketLength), -1.039);
            
            if (RayNum < 10e4)
            {
                _correctionFactor = 1.48 * 10e-2;
            } else if (10e4 <= RayNum && RayNum <= 5 * 10e5)
            {
                _correctionFactor = Math.Pow(1.5 * Math.Log(RayNum) - 5.6, -2);
            }
            else
            {
                _correctionFactor = 0.032 * Math.Pow(Rocket.RocketRoughness / Rocket.RocketLength, 0.2);
            }

            double _wetBodyDrag = 1 + 1 / (2 * Rocket.RocketFinenessRatio);
            double _wetFinDrag = 1 + (2 * Rocket.RocketFinThickness) / Rocket.RocketFinChord;
            
            this._skinFrictionDrag = _correctionFactor *
                (_wetBodyDrag * Rocket._rocketWetBody + _wetFinDrag * Rocket._rocketWetFins) / Rocket._rocketARef;
        }

        private void _calcFinPressure(double velocity)
        {
            //TODO: FIND GAMMA AND REPLACE WITH CORRECT ANGLE
            double _epsilon = 0.001;

            const double soundVelocity = 343;
            double _mach = velocity / 343;

            double _leDragCoef = Math.Pow(1 - _mach * _mach, -0.417) - 1;
            double leDragCoef = _leDragCoef * _epsilon;

            this._dragCoef =  4 * leDragCoef * (Rocket.RocketWetFins / Rocket.RocketARef) + this._skinFrictionDrag;

        }

        private void _integrateForce(double velocity)
        {
            //temp: 15, speed of sound: 340.27
            const double densityOfAir = 1.2250;

            this.dragForce = 0.5 * _dragCoef * densityOfAir * velocity * Rocket.RocketARef;
        }

        public void calDragLoop(double velocity)
        {
            double _reyNum = 10e5 * velocity * Rocket.RocketLength / 1.488;
            Debug.Log(velocity);
            _calcSkinFriction(_reyNum);
            _calcFinPressure(velocity);
            _integrateForce(velocity);
            
        }
    }
    
    
}