load('tools/jsmake-contrib/jsmake.dotnet.DotNetUtils.js');

var sys = jsmake.Sys;
var dotnet = new jsmake.dotnet.DotNetUtils();

task('default', 'test');

task('build', function () {
	dotnet.runMSBuild('src/Roger.sln', [ 'Clean', 'Rebuild' ], {Configuration: 'Release', Platform: 'Any CPU'});
});

task('test', 'build', function () {
	var runner = sys.createRunner('tools/mbunit/Gallio.Echo.exe')
		
	if(!jsmake.Sys.getEnvVar('TEAMCITY_VERSION', false))
		runner.args('/v:Verbose', '/np')
		
	runner.args('src/Tests.Unit/bin/Release/Tests.Unit.dll')
		//.args('src/Tests.Integration/bin/Release/Tests.Integration.dll')
		.run();
});