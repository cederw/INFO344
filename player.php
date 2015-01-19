<?php
class player
{
    private $PlayerName;
    private $GP;
    private $FGP;
    private $TPP;
    private $FTP;
    private $PPG;

    //Takes a string that came from the database to build the object
    public function __construct($row) {
       if(isset($row['PlayerName'])){
        $this->PlayerName = $row['PlayerName'];
       }
       if(isset($row['GP'])){
        $this->GP = $row['GP'];
       }
       if(isset($row['FGP'])){
        $this->FGP = $row['FGP'];
       }
       if(isset($row['TPP'])){
        $this->TPP = $row['TPP'];
       }
       if(isset($row['FTP'])){
        $this->FTP = $row['FTP'];
       }
       if(isset($row['PPG'])){
        $this->PPG = $row['PPG'];
       }
   } 

   function getPlayerName(){
   	return $this->PlayerName;
   }
   function getGP(){
    return $this->GP;
   }
   function getFGP(){ 
    return $this->FGP;
   }
   function getTPP(){
    return $this->TPP;
   }
   function getFTP(){
    return $this->FTP;
   }
   function getPPG(){
    return $this->PPG;
   }

}
?>