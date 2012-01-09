echo Initializing custom config...

if "!ERLANG_HOME!"=="" (
set "ERLANG_HOME=!PROGRAMFILES(x86)!\erl5.8.5\erts-5.8.5"
)

set ERLANG_SERVICE_MANAGER_PATH=!ERLANG_HOME!\bin
set RABBITMQ_BASE=%~dp0..\data
set RABBITMQ_CONFIG_FILE=%~dp0rabbitmq