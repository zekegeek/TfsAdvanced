﻿angular.module('TFS.Advanced')
    .controller('BuildStatisticsController',
    [
        '$window', '$scope', '$filter', 'buildStatisticService',
        function ($window, $scope, $filter, buildStatisticService) {
            'use strict';

            $scope.statistics = [];
            $scope.maxWaitTime = 0;
            $scope.maxRunTime = 0;
            $scope.waitTimeLabels = [];
            $scope.waitTimeSeries = [];
            $scope.waitTimes = [];
            $scope.data = {};
            $scope.daysBack = 7;

            $scope.options = {
                chart: {
                    type: "boxPlotChart",
                    height: 450,
                    maxBoxWidth : 75,
                    margin: {
                        top: 20,
                        right: 20,
                        bottom: 100,
                        left: 150
                    },
                    x: function (d) {
                        return $filter('date')(d.label);
                    },
                    yDomain: [0, 1000],
                    xAxis: {
                        axisLabel: "",
                        rotateLabels: -45
                    },
                    yAxis: {
                        axisLabel: "",
                        tickFormat: function (d) { return $scope.convertRunTime(d); }
                    }
                }
            };

            $scope.updateDaysBack = function () {
                buildStatisticService.daysBack($scope.daysBack);
            };
            

            $scope.$watchCollection(buildStatisticService.statistics,
                function (statistics) {
                    $scope.statistics = statistics;
                    var waitTimes = [];
                    var waitTimeLabels = [];

                    $scope.maxWaitTime = 0;
                    for (var index = 0; index < statistics.length; index++) {
                        var stat = statistics[index];
                        console.log(stat);
                        $scope.maxWaitTime = Math.max($scope.maxWaitTime, stat.queueTimeMax);
                        $scope.maxRunTime = Math.max($scope.maxRunTime, stat.runTimeMax);
                        waitTimes.push({
                            label: new Date(stat.day),
                            values: {
                                Q1: stat.queueTimesLowerPercentile,
                                Q2: stat.queueTimeAverage,
                                Q3: stat.queueTimesUpperPercentile,
                                whisker_low: stat.queueTimeMin,
                                whisker_high: stat.queueTimeMax,
                                outliers: [stat.runCount]
                            }
                        });
                    
                        waitTimeLabels.push(new Date(stat.day));
                    }
                    $scope.options.chart.yDomain = [0, $scope.maxWaitTime];
                    $scope.data = waitTimes;
                    $scope.waitTimeLabels = waitTimeLabels;

                });

            
            
            $scope.convertRunTime = function (runTime) {

                var secs = Math.round((runTime) % 60);
                var mins = Math.trunc(((runTime) / 60));
                var time = "";
                if (mins > 0)
                    time += mins + " mins ";
                if (secs > 0)
                    time += secs + " seconds";

                return  time;
            };

        }]);