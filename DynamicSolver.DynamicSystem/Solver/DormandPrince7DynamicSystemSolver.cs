using DynamicSolver.Expressions.Execution;
using JetBrains.Annotations;

namespace DynamicSolver.DynamicSystem.Solver
{
    public class DormandPrince7DynamicSystemSolver : ButcherTableDynamicSystemSolver
    {
        private static readonly double[,] ACoefficients = {
            {                      0,     0,       0,                         0,                      0,                        0,                        0,                        0,                        0,                      0,                     0,     0},
            {                  1d/18,     0,       0,                         0,                      0,                        0,                        0,                        0,                        0,                      0,                     0,     0},
            {                  1d/48, 1d/16,       0,                         0,                      0,                        0,                        0,                        0,                        0,                      0,                     0,     0},
            {                  1d/32,     0,   3d/32,                         0,                      0,                        0,                        0,                        0,                        0,                      0,                     0,     0},
            {                  5d/16,     0, -75d/64,                    75d/64,                      0,                        0,                        0,                        0,                        0,                      0,                     0,     0},
            {                  3d/80,     0,       0,                     3d/16,                  3d/20,                        0,                        0,                        0,                        0,                      0,                     0,     0},            
            {    29443841d/614563906,     0,       0,       77736538d/692538347,  -28693883d/1125000000,     23124283d/1800000000,                        0,                        0,                        0,                      0,                     0,     0},
            {    16016141d/946692911,     0,       0,       61564180d/158732637,    22789713d/633445777,    545815736d/2771057229,   -180193667d/1043307555,                        0,                        0,                      0,                     0,     0},
            {    39632708d/573591083,     0,       0,     -433636366d/683701615, -421739975d/2616292301,     100302831d/723423059,     790204164d/839813087,    800635310d/3783071287,                        0,                      0,                     0,     0},
            {  246121993d/1340847787,     0,       0, -37695042795d/15268766246, -309121744d/1061227803,     -12992083d/490766935,   6005943493d/2108947869,    393006217d/1396673457,    123872331d/1001029789,                      0,                     0,     0},
            { -1028468189d/846180014,     0,       0,     8478235783d/508512852, 1311729495d/1432422823, -10304129995d/1701304382, -48777925059d/3047939560,  15336726248d/1032824649, -45442868181d/3398467696,  3065993473d/597172653,                     0,     0},
            {   185892177d/718116043,     0,       0,    -3185094517d/667107341, -477755414d/1098053517,    -703635378d/230739211,   5731566787d/1027545527,    5232866602d/850066563,   -4093664535d/808688257, 3962137247d/1805957418,   65686358d/487910083,     0},
            {   403863854d/491063109,     0,       0,    -5068492393d/434740067,  -411421997d/543043805,     652783627d/914296604,   11173962825d/925320556, -13158990841d/6184727034,   3936647629d/1978049680,  -160528059d/685178525, 248638103d/1413531060,     0}
        };

        private static readonly double[] BCoefficients =
        {
            13451932d/455176623, 0, 0, 0, 0, -808719846d/976000145, 1757004468d/5645159321, 656045339d/265891186, -3867574721d/1518517206, 465885868d/322736535, 53011238d/667516719, 2d/45, 0
        };

        protected override double[,] A => ACoefficients;
        protected override double[] B => BCoefficients;

        public DormandPrince7DynamicSystemSolver([NotNull] IExecutableFunctionFactory functionFactory) : base(functionFactory)
        {
            
        }
    }
}