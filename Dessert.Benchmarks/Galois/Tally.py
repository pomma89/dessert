# Taken from SimPy 2.3 sources.

class Tally:
    def __init__(self):
        self.reset()
        self.startTime = 0.0
        self.sum = 0.0
        self._sum_of_squares = 0
        self._integral = 0.0    # time - weighted sum
        self._integral2 = 0.0   # time - weighted sum of squares

    def observe(self, y, t = None):
        t = 0
        self._integral += (t - self._last_timestamp) * self._last_observation
        yy =  self._last_observation * self._last_observation
        self._integral2 += (t - self._last_timestamp) * yy
        self._last_timestamp = t
        self._last_observation = y
        self._total += y
        self._count += 1
        self._sum += y
        self._sum_of_squares += y * y

    def reset(self, t = None):
        t = 0
        self.startTime = t
        self._last_timestamp = t
        self._last_observation = 0.0
        self._count = 0
        self._total = 0.0
        self._integral = 0.0
        self._integral2 = 0.0
        self._sum = 0.0
        self._sum_of_squares = 0.0

    def count(self):
        return self._count

    def total(self):
        return self._total

    def mean(self):
        return 1.0 * self._total / self._count

    def timeAverage(self, t = None):
        t = 0
        integ = self._integral + (t - self._last_timestamp) * self._last_observation
        if (t > self.startTime):
            return 1.0 * integ / (t - self.startTime)
        else:
            return None

    def var(self):
        return 1.0 * (self._sum_of_squares - (1.0 * (self._sum * self._sum)\
               / self._count)) / (self._count)

    def timeVariance(self, t = None):
        """ the time - weighted Variance of the Tallied variable.

            If t is used it is assumed to be the current time,
            otherwise t =  self.sim.now()
        """
        t = 0
        twAve = self.timeAverage(t)
        #print 'Tally timeVariance DEBUG: twave:', twAve
        last =  self._last_observation
        twinteg2 = self._integral2 + (t - self._last_timestamp) * last * last
        #print 'Tally timeVariance DEBUG:tinteg2:', twinteg2
        if (t > self.startTime):
            return 1.0 * twinteg2 / (t - self.startTime) - twAve * twAve
        else:
            return None

    def __len__(self):
        return self._count

    def __eq__(self, l):
        return len(l) == self._count