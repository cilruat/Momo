<?php
include("connect.php");

$group_id = $_POST['group_id'];

$query = "select * from momo_db.group where id='$group_id'";

$result = mysqli_query($con, $query);
$response = mysqli_fetch_assoc($result);
mysqli_close($con);
echo json_encode($response);
?>