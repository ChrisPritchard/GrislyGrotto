(function () {

    var dates = [];
    var parCounts = [];
    for (i = 1; i <= 30; i++) {
        dates.push('2017-11-' + i);
        var adjustment = Math.floor(i / 3) * -1;
        if (i % 3 !== 0)
            adjustment -= 1;
        parCounts.push(i * 1667 + adjustment);
    }

    Chart.defaults.global.defaultFontColor = "whitesmoke";

    function getApiResult(username, onSuccess) {
        var request = new XMLHttpRequest();
        request.open('GET', '/wrimo/' + username, true);
        
        request.onload = function() {
            if (request.status < 200 || request.status >= 400)
                return; // failure
            var data = JSON.parse(request.responseText);
            onSuccess(data);
        };
        
        request.send();
    }

    function renderChart(canvas) {
        var userName = canvas.getAttribute('data-wrimo');
        getApiResult(userName, function (wrimoData) {
            var counts = [];
            var runningTotal = 0;
            for (var j in wrimoData.wordcounts) {
                runningTotal += wrimoData.wordcounts[j].wc;
                counts.push(runningTotal);
            }

            // account for the api's daily count array being 12 hours or so behind NZ
            if (runningTotal != wrimoData.user_wordcount)
                counts.push(wrimoData.user_wordcount);

            var prev = canvas.previousElementSibling;
            prev.innerText = wrimoData.uname;
            prev.setAttribute('href', 'http://nanowrimo.org/participants/' + userName);
            canvas.nextElementSibling.innerText = 'Total: ' + wrimoData.user_wordcount + ' words';
            
            new Chart(canvas, {
                type: 'bar',
                data: {
                    labels: dates,
                    datasets: [
                        {
                            type: 'bar',
                            label: 'Wordcount',
                            data: counts,
                            backgroundColor: 'red',
                            borderWidth: 1
                        },
                        {
                            type: 'line',
                            label: 'Par',
                            backgroundColor: 'rgba(200, 200, 200, 0.5)',
                            data: parCounts
                        }
                    ]
                },
                options: {
                    maintainAspectRatio: false,
                    scales: {
                        yAxes: [{
                            ticks: {
                                beginAtZero: true, 
                                max: 60000
                            }
                        }]
                    }
                }
            });

            document.getElementById('wrimo-header').style.display = '';
        });
    }

    var elements = document.querySelectorAll('canvas[data-wrimo]');
    Array.prototype.forEach.call(elements, function(el) {
        renderChart(el);
    });
})();