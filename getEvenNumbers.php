<html>
<head>
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
</head>
<body>
	<?php
	if(isset($_GET["n"])){
?>

	<div class="container">
		 

		 <div class="col-md-6">
		 	<h3>Even numbers</h3>
		 	<ul>
<?php
for($i = 2; $i<=$_GET["n"];$i = $i+2){
	
	echo "<li>".$i."</li>";

}
?>
</ul>
</div>
<div class="col-md-6">
<h3>Prime numbers</h3>
		 	<ul>
<?php
for($i = 2; $i<$_GET["n"];$i++){
	$bool = TRUE;
	for($j = 2; $j<$i;$j++){
		if($i%$j==0){
			$bool = FALSE;
		}
	}
	if($bool){
		echo "<li>".$i."</li>";
	}
	

}
?>
</ul>
		 </div>
		</div>
<?php
}
		?>
		 <div class="container">
<form action="getEvenNumbers.php" method="GET">
	<div class="form-group">
	<label>Enter A number</label>
<input type="text" name="n" class="form-control">
</div>
<input type="submit" value="Submit" class="btn btn-default">

</form>
<div>
</div>
<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js"></script>
<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"></script>
</body>
</html>

