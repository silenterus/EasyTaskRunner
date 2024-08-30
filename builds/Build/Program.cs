using static Build.Commands;
using static Bullseye.Targets;

AddTargets(args);

await RunTargetsAndExitAsync(args);
