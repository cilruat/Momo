<?php
include("connect.php");

$id = $_POST['id'];
$name = $_POST['name'];
$description = $_POST['description'];
$start_date = $_POST['start_date'];
$end_date = $_POST['end_date'];
$color = $_POST['color'];
$alarm_type = $_POST['alarm_type'];

$query = "update momo_db.`schedule` set name='$name', description='$description', start_date='$start_date', end_date='$end_date', color='$color', alarm_type='$alarm_type' where id='$id'";

$response = mysqli_query($con, $query);
$success = $response == 1 ? true : false;

mysqli_close($con);
echo json_encode($response);
?>