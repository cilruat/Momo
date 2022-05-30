<?php
include("connect.php");

$person_ids = $_POST['person_ids'];

$query = "select * from momo_db.person where ";

$split_ids = explode(',', $person_ids);
$split_length = count($split_ids);
for ($i = 0; $i < $split_length; $i++)
{
	$query .= "p_id='$split_ids[$i]'";
	if ($i < $split_length - 1)
		$query .= " or ";
}

$result = mysqli_query($con, $query);

if (mysqli_num_rows($result))
{
	$n = 0;	
	while ($db = mysqli_fetch_assoc($result))
	{
		$response[$n] = $db;
		++$n;
	}
}

mysqli_close($con);
echo json_encode($response);
?>