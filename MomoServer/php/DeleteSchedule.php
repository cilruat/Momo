<?php
include("connect.php");

$id = $_POST['id'];

$query = "update momo_db.`schedule` set `delete`=1 where id='$id'";
$response = mysqli_query($con, $query);

mysqli_close($con);
echo json_encode($response);
?>