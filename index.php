
<!DOCTYPE HTML>
<!--Walter Ceder INFO 344 PA1-->
<html>
<head>
	<meta charset="UTF-8">
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/css/bootstrap.min.css">
</head>

<body>
<nav class="navbar navbar-default">
  <div class="container-fluid">

    <div class="navbar-header">
      <a class="navbar-brand" href="#">
        <img alt="Brand" src="nba-logo.png">
      </a>
    </div>
    <div class="navbar-header">

      <a class="navbar-brand" href="#">NBA Player Stats</a>
    </div>
  </div>
</nav>
<div class="container">
	<div class="jumbotron">
		<form action="index.php" method="GET">
			<div class="form-group">
			    <label for="name">Player Name</label>
			    <input type="text" class="form-control" id="name" name="name" placeholder="Enter name">
			    
			 </div>
			 <button type="submit" class="btn btn-default">Submit</button>	
		</form>
	</div>
</div>

<div class="container">
<?php
include("player.php");
try {
	//Only runs this code if a search has been preformed
	if(isset($_GET['name'])){
		$searchName = $_GET['name'];
		$players  = array();	    
	    $conn = new PDO('mysql:host=pa1.czoayjspkwkp.us-west-2.rds.amazonaws.com;dbname=nba', 'info344user', 'Oliveisc00');
	    $conn->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
		//levenshtein - gets the closest name to the search string
	    $data = $conn->prepare('SELECT `PlayerName` FROM `stats`');
	 	$data->execute();
	 	$closestMatch = "error";
	 	$clostestLDistance = 1000;
	 	foreach($data as $row) {
	 		$substrings = preg_split("/\s/",$row['PlayerName']);
	 		array_push($substrings,$row['PlayerName']);
	 		
	 		foreach($substrings as $substring){
	 			
	 			$tempLDistance = levenshtein(strtolower($searchName), strtolower($substring));
		 		if($tempLDistance<=$clostestLDistance){

		 			$closestMatch = $substring;
		 			$clostestLDistance = $tempLDistance;
		 		}
	 		}
	 		
	 	}

		//displayy the results for the closest name in a list
		
	 
	    $data = $conn->prepare('SELECT * FROM `stats` WHERE PlayerName LIKE :parameter');
	    $likeName = '%'.$closestMatch.'%';
	    $data->bindParam(':parameter', $likeName, PDO::PARAM_STR);
	 	$data->execute();
	 	?>
	 		<h3>Results for: <?php echo $searchName ?></h3>
	 		<?php
	 		if($clostestLDistance!=0){
	 			?>
	 				<h4>Not found, showing results for: <em><?php echo $closestMatch ?></em></h4>
	 			<?php
	 		}
	 		?>
	 	<?php
	    foreach($data as $row) {
	    	$name = $row['PlayerName'];
	    	$players[$name]  = new player($row);
	        ?>
	        <div class="panel panel-default">
			  <div class="panel-heading">
			    <h3 class="panel-title"><?php echo $players[$name]->getPlayerName()?></h3>
			  </div>
			  <div class="panel-body">
			    <table class="table table-striped table-hover">
				  <tr>
				  	<th>
				  		GP
				  	</th>
				  	<th>
				  		FGP
				  	</th>
				  	<th>
				  		TPP
				  	</th>
				  	<th>
				  		FTP
				  	</th>
				  	<th>
				  		PPG
				  	</th>
				  </tr>
				  <tr>
				  	<td>
				  		<?php echo $players[$name]->getGP()?>
				  	</td>
				  	<td>
				  		<?php echo $players[$name]->getFGP()?>
				  	</td>
				  	<td>
				  		<?php echo $players[$name]->getTPP()?>
				  	</td>
				  	<td>
				  		<?php echo $players[$name]->getFTP()?>
				  	</td>
				  	<td>
				  		<?php echo $players[$name]->getPPG()?>
				  	</td>
				  </tr>
				</table>
			  </div>
			</div>

			<?php
	    }
	} else{
		//when no one has been searched for do nothing
	}
} catch(PDOException $e) {
    echo 'ERROR: ' . $e->getMessage();
}
?>
</div>
  	<script src="//code.jquery.com/jquery-1.11.2.min.js"></script>
	<script src="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"></script>
</body>

