@ECHO OFF

IF NOT [%1]==[] (
	ECHO %*
	ECHO:
)

SET /A args_cnt=0

:LOOP
	IF [%1]==[] GOTO END
	SET /A args_cnt+=1
	ECHO Arg%args_cnt%:(%1)
	SHIFT
	GOTO LOOP

:END
	ECHO:
	ECHO %args_cnt% Argument(s).
