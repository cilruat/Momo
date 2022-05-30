<?php
function post_request($data)
{
	$data = http_build_query($data);
	$url = parse_url("http://192.168.0.7");
	
	if ($url['scheme'] != 'http')
		return "Error: Only HTTP request are supported!";
	
	$host = $url['host'];
	$path = $url['path'];
	$res = '';
	
	if ($fp = fsockopen($host, 7979, $errno, $errstr, 300))
	{
		$reqBody = $data;
		$reqHeader = "POST $path HTTP/1.1\r\n"."Host: $host\r\n";
		$reqHeader .= "Content-Type: application/x-www-form-urlencoded\r\n"
					."Content-Length: ".strlen($reqBody)."\r\n"
					."Connection: close\r\n\r\n";
					
		fwrite($fp, $reqHeader);
		fwrite($fp, $reqBody);
		
		while(!feof($fp))
			$res .= fgets($fp, 1024);
		
		fclose($fp);
	}
	else
		return "Error: Cannot Connect!";
	
	$result = explode("\r\n\r\n", $res, 2);

	$header = isset($result[0]) ? $result[0] : '';
	$content = isset($result[1]) ? $result[1] : '';

	return $content;
}
?>