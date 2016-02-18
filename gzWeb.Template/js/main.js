$(document).ready(function(){
	var more = false;
	var more1 = false;
	var more2 = false;
	$('#b1 img').click(function() {
		var text = $('.more');
		if (text.is(':hidden')) {
			text.slideDown('200');
			$('#b1 img').removeClass('rot-up');
			$('#b1 img').addClass('rot-down');
			$('#pth').hide('fast');
			more=true;
		} else {
			text.slideUp('200');
			$('#b1 img').removeClass('rot-down');
			$('#b1 img').addClass('rot-up');
			if(more2==false){
				$('#pth').show('slow');
			}
			more=false;
		}
	});
	$('#b2').click(function() {
		var text = $('.more1');
		if (text.is(':hidden')) {
			text.slideDown('200');
			$('#b2 img').removeClass('rot-up');
			$('#b2 img').addClass('rot-down');
			$('#pth').hide('fast');
			more2=true;
		} else {
			text.slideUp('200');
			$('#b2 img').removeClass('rot-down');
			$('#b2 img').addClass('rot-up');
			if(more==false){
				$('#pth').show('slow');
			}
			more2=false;
		}
	});
	$('#b3').click(function() {
		var text = $('.more2');
		if (text.is(':hidden')) {
			text.slideDown('200');
			$('#b3 img').removeClass('rot-up');
			$('#b3 img').addClass('rot-down');
			more3=true;
		} else {
			text.slideUp('200');
			$('#b3 img').removeClass('rot-down');
			$('#b3 img').addClass('rot-up');
			more3=false;
		}
	});


	var i=0;
	var r= -64;
	var interval1;
	var interval2;
	var interval3;
	var sel1 = false;
	var sel2 = false;
	var sel3 = false;
	
	$('#pl1').click(function() {
			clearInterval(interval2); 
			clearInterval(interval3); 
			$( ".s-mod").hide();
			$( ".s-con").hide();
			$('#pl2').removeClass('ison');
			$('#pl3').removeClass('ison');
			$('#pl2').text("SELECT");
			$('#pl3').text("SELECT");
			sel2 = false;
			sel3 = false;
			
			if (sel1==false) {	

			$('#pl1').text("SELECTED");
			$('#pl1').addClass('ison');
			$(this).prop('checked', true);

			interval1 = setInterval(function(){
			$( ".s-agg").show();
			var is=i.toString();
			$( ".move2").attr('x', is);
			i++;
			if (i==64){
		  	i=0;  // more statements
			}
			},10);

			sel1 = true;
			
				
			} else {		
				$(this).prop('checked', false);
				$('#pl1').text("SELECT");
				$('#pl1').removeClass('ison');
				$( ".s-agg").hide();
				clearInterval(interval1); 

				sel1 = false;
				
			}
			console.log(sel1);			
		
	});

	$('#pl2').click(function() {
			clearInterval(interval1); 
			clearInterval(interval3); 
			$( ".s-agg").hide();
			$( ".s-con").hide();
			$('#pl1').removeClass('ison');
			$('#pl3').removeClass('ison');
			$('#pl1').text("SELECT");
			$('#pl3').text("SELECT");
			sel1 = false;
			sel3 = false;

			
			if (sel2==false) {	

			$('#pl2').text("SELECTED");
			$('#pl2').addClass('ison');
			$(this).prop('checked', true);

			interval2 = setInterval(function(){
			$( ".s-mod").show();
			var is=i.toString();
			$( ".move3").attr('x', is);
			i++;
			if (i==64){
		  	i=0;  // more statements
			}
			},10);

			sel2 = true;
				
			} else {		
				$(this).prop('checked', false);
				$('#pl2').text("SELECT");
				$('#pl2').removeClass('ison');
				$( ".s-mod").hide();
				clearInterval(interval2); 

				sel2 = false;
				
			}
			console.log(sel2);			
		
	});

	$('#pl3').click(function() {
			clearInterval(interval1); 
			clearInterval(interval2); 
			$( ".s-agg").hide();
			$( ".s-mod").hide();
			$('#pl1').removeClass('ison');
			$('#pl2').removeClass('ison');
			$('#pl1').text("SELECT");
			$('#pl2').text("SELECT");
			sel1 = false;
			sel2 = false;
			
			if (sel3==false) {	

			$('#pl3').text("SELECTED");
			$('#pl3').addClass('ison');
			$(this).prop('checked', true);

			interval3 = setInterval(function(){
			$( ".s-con").show();
			var is=r.toString();
			$( ".move").attr('x', is);
			r++;
			if (r==0){
		 	r=-64;  // more statements
			}
			},10);

			sel3 = true;
				
			} else {		
				$(this).prop('checked', false);
				$('#pl3').text("SELECT");
				$('#pl3').removeClass('ison');
				$( ".s-con").hide();
				clearInterval(interval3); 

				sel3 = false;
				
			}
			console.log(sel3);			
		
	});

	//$('#pl1').focus(function() {
		//$( "#pl1" ).unbind( "mouseenter" );
		
	//});
	//$('#pl1').blur(function() {
		
		//$( "#pl1" ).bind( "mouseenter" );
	//});

	
	//$( "#pl1" )
	//.mouseenter(function() {
			//interval1 = setInterval(function(){
			//$( ".s-agg").show();
			//var is=i.toString();
			//$( ".move2").attr('x', is);
			//i++;
			//if (i==64){
		  	//i=0;  // more statements
			//}

			//},10);
		
	//})
	//.mouseleave(function() {
		//$( ".s-agg").hide();
		//clearInterval(interval1); 
	//});



	//$( "#pl2" )
	//.mouseenter(function() {
		//interval2 = setInterval(function(){
			//$( ".s-mod").show();
			//var is=i.toString();
			//$( ".move3").attr('x', is);
			//i++;
			//if (i==64){
		  //i=0;  // more statements
		//}

	//},10);
		
	//})
	//.mouseleave(function() {
		//$( ".s-mod").hide();
		//clearInterval(interval2); 
	//});


	//$( "#pl3" )
	//.mouseenter(function() {
		//interval3 = setInterval(function(){
			//$( ".s-con").show();
			//var is=r.toString();
			//$( ".move").attr('x', is);
			//r++;
			//if (r==0){
		 // r=-64;  // more statements
		//}

	//},10);
		
	//})
	//.mouseleave(function() {
	//	$( ".s-con").hide();
	//	clearInterval(interval3); 
	//});
	

});
