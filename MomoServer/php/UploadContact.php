<?php
include("connect.php");

try
{
	$id = $_POST['id'];
	
	$upload_dir = '../file/contact/person_'.$id;	
	if (!is_dir($upload_dir))
		mkdir($upload_dir, 0777, true);
		
	$files = $_FILES['fileToUpload']['name'];		
	$file_name = basename($files);
			
	$upload_file = $upload_dir."/".$file_name;
	
	$temp_files = $_FILES['fileToUpload']['tmp_name'];
	move_uploaded_file($temp_files, $upload_file);
}
catch(exception $e)
{
}
?>