@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
@ Please globally replace following "TargetSchema" @
@ with your actual target schema for installation, @
@ and then remove this block of comment (5 lines). @
@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

CREATE OR REPLACE PACKAGE "TargetSchema".REST_UTILITY IS

-- ================================================================
-- Description:		A sample utility for PL/SQL to call RESTful Service
-- Repository:		https://github.com/DataBooster/PS-WebApi
-- Original Author:	Abel Cheng <abelcys@gmail.com>
-- Create date:		2017-11-05
-- ================================================================

FUNCTION INVOKE_WEBAPI
(	-- Source: https://github.com/DataBooster/PS-WebApi/blob/master/sample/PLSQL-Client/REST_UTILITY.pck
	url				VARCHAR2,
	post_content	VARCHAR2		:= NULL,
	content_type	VARCHAR2		:= 'application/json',
	accept_type		VARCHAR2		:= 'application/json',
	timeout_sec		SIMPLE_INTEGER	:= 600
)	RETURN CLOB;

END REST_UTILITY;

/
CREATE OR REPLACE PACKAGE BODY "TargetSchema".REST_UTILITY IS

FUNCTION INVOKE_WEBAPI
(	-- Source: https://github.com/DataBooster/PS-WebApi/blob/master/sample/PLSQL-Client/REST_UTILITY.pck
	url				VARCHAR2,
	post_content	VARCHAR2		:= NULL,
	content_type	VARCHAR2		:= 'application/json',
	accept_type		VARCHAR2		:= 'application/json',
	timeout_sec		SIMPLE_INTEGER	:= 600
)	RETURN CLOB		IS
contentLength	PLS_INTEGER		:= LENGTH(post_content);
httpReq			UTL_HTTP.REQ;
httpResp		UTL_HTTP.RESP;
httpMethod		VARCHAR2(16);
respBuffer		VARCHAR2(32767);
bufferLen		SIMPLE_INTEGER	:= 32767 - 1;
respClob		CLOB;
BEGIN
	IF contentLength > 0 THEN
		httpMethod	:= 'POST';
	ELSE
		httpMethod	:= 'GET';
	END IF;

	UTL_HTTP.SET_TRANSFER_TIMEOUT(timeout_sec);

	httpReq	:= UTL_HTTP.BEGIN_REQUEST(url, httpMethod, UTL_HTTP.HTTP_VERSION_1_1);

	UTL_HTTP.SET_HEADER(httpReq, 'User-Agent', 'Mozilla/4.0');

	IF content_type IS NOT NULL THEN
		UTL_HTTP.SET_HEADER(httpReq, 'Content-Type', content_type);
	END IF;

	IF accept_type	IS NOT NULL THEN
		UTL_HTTP.SET_HEADER(httpReq, 'Accept', accept_type);
	END IF;

	IF contentLength > 0 THEN
		UTL_HTTP.SET_HEADER(httpReq, 'Content-Length', contentLength);
		UTL_HTTP.WRITE_TEXT(httpReq, post_content);
	END IF;

	httpResp	:= UTL_HTTP.GET_RESPONSE(httpReq);

	BEGIN
		LOOP
			UTL_HTTP.READ_TEXT(httpResp, respBuffer, bufferLen);
			respClob	:= respClob || respBuffer;
		END LOOP;
		EXCEPTION
			WHEN UTL_HTTP.END_OF_BODY THEN
				UTL_HTTP.END_RESPONSE(httpResp);
	END;

	RETURN respClob;

EXCEPTION
	WHEN OTHERS THEN
		IF httpResp.status_code > 0 THEN
			UTL_HTTP.END_RESPONSE(httpResp);
		END IF;
	    RAISE;
END INVOKE_WEBAPI;

END REST_UTILITY;
/
