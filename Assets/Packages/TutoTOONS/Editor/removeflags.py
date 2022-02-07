import os
from subprocess import call

_path_project = "/Users/tutotoonsgenerator/Documents/git/tutotoons_generator/generator_server/_generator_unity/_generator/RemoveOtherLinkerFlags"
_path_unity = "/Applications/Unity/Hub/Editor/2019.4.4f1/Unity.app/Contents/MacOS/Unity"
_current_proj = os.path.abspath(os.getcwd())

_args = []
_args.append(_path_unity)
_args.append("-projectPath")
_args.append(_path_project)
_args.append("-buildTarget")
_args.append("iOS")
_args.append("-executeMethod")
_args.append("Tools.RemoveOtherLinkerFlags")
_args.append("-batchmode")
_args.append("-nographics")
_args.append("-quit")
_args.append("-xcode_path")
_args.append(_current_proj)
call(_args)