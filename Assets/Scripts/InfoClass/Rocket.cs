namespace DefaultNamespace.InfoClass
{
    public static class Rocket
    {
        public static double _rocketARef = 0.7795;
        public static double _rocketFinenessRatio = 12;
        public static double _rocketFinThickness = 2;
        public static double _rocketWetBody = 0.7539;
        public static double _rocketWetFins = 0.0256;
        public static double _rocketFinChord = 0.08;
        public static double _rocketRoughness = 12;
        public static double _rocketLength = 1.2;
        
        public static double RocketARef
        {
            get => _rocketARef;
            set => _rocketARef = value;
        }
        
        public static double RocketRoughness
        {
            get => _rocketRoughness;
            set => _rocketRoughness = value;
        }
        
        public static double RocketLength
        {
            get => _rocketLength;
            set => _rocketLength = value;
        }

        public static double RocketFinenessRatio
        {
            get => _rocketFinenessRatio;
            set => _rocketFinenessRatio = value;
        }

        public static double RocketFinThickness
        {
            get => _rocketFinThickness;
            set => _rocketFinThickness = value;
        }

        public static double RocketWetBody
        {
            get => _rocketWetBody;
            set => _rocketWetBody = value;
        }

        public static double RocketWetFins
        {
            get => _rocketWetFins;
            set => _rocketWetFins = value;
        }

        public static double RocketFinChord
        {
            get => _rocketFinChord;
            set => _rocketFinChord = value;
        }
    }
}