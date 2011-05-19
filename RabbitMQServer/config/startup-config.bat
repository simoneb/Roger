echo Initializing custom config...

set ERLANG_HOME=!PROGRAMFILES!\erl5.8.3\erts-5.8.3
set ERLANG_SERVICE_MANAGER_PATH=!ERLANG_HOME!\bin
set RABBITMQ_BASE=%~dp0..\data
set RABBITMQ_CONFIG_FILE=%~dp0rabbitmq