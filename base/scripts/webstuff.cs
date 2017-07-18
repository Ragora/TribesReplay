
function getLinkName(%line, %offset)
{
   if(%offset $= "")
      %offset = 0;

   %name = getField(%line, %offset);
   %tag = getField(%line, %offset + 1);
   %append = getField(%line, %offset + 2);
   %uid = getField(%line, %offset + 3);

   if(%append)
      %str = "<color:FFFFFF>" @ %name @ "<color:FFFF00>" @ %tag;
   else
      %str = "<color:FFFF00>" @ %tag @ "<color:FFFFFF>" @ %name;
   return "<a:player" TAB %name TAB %uid @ "><spush>" @ %str @ "<spop></a>";
}


function getTextName(%line, %offset)
{
   if(%offset $= "")
      %offset = 0;

   %name = getField(%line, %offset);
   %tag = getField(%line, %offset + 1);
   %append = getField(%line, %offset + 2);
   %uid = getField(%line, %offset + 3);

   if(%append)
      return %name @ %tag TAB %uid;
   else
      return %tag @ %name TAB %uid;
}

function HTTPRequest(%script, %update, %destObject, %key)
{
   error("Call to HTTP request depricated - trace:");
   trace(1);
   schedule(0,0,trace,0);

   //%upd = new SecureHTTPObject(HTTPUpdate);
   //%upd.post($WebAddress, $WebPath @ %script @ ".php", "", %update);
   //%upd.key = %key;
   //%upd.destObject = %destObject;
}

function HTTPSecureRequest(%script, %update, %destObject, %key)
{
   error("Call to HTTP secure request depricated - trace:");
   trace(1);
   schedule(0,0,trace,0);

   //%upd = new SecureHTTPObject(HTTPUpdate);
   //%upd.setAuthenticated();
   //%upd.post($WebAddress, $WebPath @ %script @ ".php", "", %update);
   //%upd.key = %key;
   //%upd.destObject = %destObject;
}

$DBNextQueryId = 0;
$DBQueryCount = 0;

if(!isObject(ProxyEchoTest))
{
   new ScriptObject(ProxyEchoTest)
   {
      class = ProxyEcho;
   };
}

function ProxyEcho::onDatabaseQueryResult(%this, %status, %string, %key)
{
   echo("Got database query result for key: " @ %key);
   echo("Status = " @ %status);
   echo("String/Rowcount = " @ %string);
}

function ProxyEcho::onDatabaseRow(%this, %row, %isLast, %key)
{
   echo("Got Row - key = " @ %key @ " isLast = " @ %isLast);
   echo("Row Text = " @ %row);
}

function HandleDatabaseProxyResponse(%prefix, %params)
{
   %id = getWord(%params, 0);

   for(%qc = 0; %qc < $DBQueryCount; %qc++)
      if(getWord($DBQueries[%qc], 0) == %id)
         break;
   if(%qc == $DBQueryCount)
   {
      warn("Invalid database proxy message id: " @ %id);
      return;
   }

   %lastPacket = getWord(%params, 1);
   %start = strpos(%params, ":") + 1;
   if(!%start)
   {
      warn("Invalid database proxy message: " @ %params);
      return;
   }

   // convert all the escaped characters
   %newStr = getSubStr(%params, %start, 100000);
   %newStr = strreplace(%newStr, "\\n", "\n");
   %newStr = strreplace(%newStr, "\\\\", "\\");

   // concat it with any text from prior message(s) that hasn't been
   // processed yet.
   %msg = $DBQueryText[%qc] @ %newStr;
   %proxyObject = getWord($DBQueries[%qc], 2);
   %proxyKey = getWord($DBQueries[%qc], 3);

   if(getWord($DBQueries[%qc], 1) == 0) // we haven't receivd the first 2 recs yet
   {
      // check if we have 2 records in %msg:
      %pos1 = strpos(%msg, "\\S");
      if(%pos1 != -1)
         %pos2 = strpos(%msg, "\\S", %pos1 + 2);

      if(%pos1 != -1 && %pos2 != -1)
      {
         %resultStatus = getSubStr(%msg, 0, %pos1);
         %resultString = getSubStr(%msg, %pos1 + 2, %pos2 - (%pos1 + 2));
         if(%proxyObject !$= "")
            %proxyObject.onDatabaseQueryResult(%resultStatus, %resultString, %proxyKey);
         $DBQueries[%qc] = setWord($DBQueries[%qc], 1, "1");
         %msg = getSubStr(%msg, %pos2 + 2, 100000);
      }
   }
   if(getWord($DBQueries[%qc], 1) == 1)
   {
      // start spitting out rows:
      while((%pos = strpos(%msg, "\\S")) != -1)
      {
         %row = getSubStr(%msg, 0, %pos);
         %msg = getSubStr(%msg, %pos + 2, 100000);

         if(%proxyObject !$= "")
            %proxyObject.onDatabaseRow(%row, %msg $= "" && %lastPacket, %proxyKey);
      }
   }
   $DBQueryText[%qc] = %msg; // save off the last text...
   if(%lastPacket)
   {
      // erase the query from the array:
      if(getWord($DBQueries[%qc], 1) == 0)
         warnf("Error in database query response - not enough data.");
      for(%i = %qc; %i < $DBQueryCount; %i++)
      {
         $DBQueries[%i] = $DBQueries[%i+1];
         $DBQueryText[%i] = $DBQueryText[%i+1];
      }
      $DBQueryCount--;
   }
}

// DBQuery is: id recCountRecvd proxyObject key

function DatabaseQueryi(%astr, %args, %proxyObject, %key)
{
   // maxRows will be empty
   $NextDatabaseQueryId++;
   %id = $NextDatabaseQueryId;

   $DBQueries[$DBQueryCount] = %id @ " 0 " @  %proxyObject SPC %key;
   $DBQueryText[$DBQueryCount] = "";
   $DBQueryCount++;

   %strStart = 0;
   %args = strreplace(%args, "\r", "");
   %args = strreplace(%args, "\\", "\\\\");
   %args = strreplace(%args, "\n", "\\n");

   echo("Called DBQueryi - astr: " @ %astr @ " args: " @ %args);
   
   %len = strlen(%args);
   %i = 0;
   while(%i+400 < %len)
   {
      %msg = getSubStr(%args, %i, 400);
      IRCClient::send("dbqa" SPC %id SPC ":" @ %msg);
      %i += 400;
   }
   %msg = getSubStr(%args, %i, 400);
   IRCClient::send("dbqax" SPC %id SPC %astr SPC ":" @ %msg);
}

function DatabaseQuery(%ordinal, %args, %proxyObject, %key)
{
   DatabaseQueryi(%ordinal SPC "0", %args, %proxyObject, %key);
}

function DatabaseQueryArray(%ordinal, %maxRows, %args, %proxyObject, %key)
{
   DatabaseQueryi("C" @ %ordinal SPC %maxRows, %args, %proxyObject, %key);
}

function WONUpdateCertificateDone(%errCode, %errStr)
{
   echo("Got cert back from WON: " @ %errCode @ " = " @ %errStr);
}