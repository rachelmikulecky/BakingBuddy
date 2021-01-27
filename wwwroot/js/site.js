// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

const denominators = [2, 3, 4, 6, 8, 16];
// Have to double escape any special character
const startsWithDigit = new RegExp('^\\d');
const fraction = new RegExp("^(\\d+\\s)?(\\d+\\/\\d+)([\\s\\S]*)");
const decimal = new RegExp("^(\\d+\\.\\d+)([\\s\\S]*)");
const integer = new RegExp("^(\\d+)([\\s\\S]*)");
const range = new RegExp("^(\\d+\\/\\d+|\\d+\\.\\d+|\\d+)\\s*(to|\\-)\\s*(\\d+\\/\\d+|\\d+\\.\\d+|\\d+)([\\s\\S]*)");

$("#factor").on('keyup', function (e) {
    if (e.key === 'Enter' || e.keyCode === 13) {
        // Stop default action
    }
});

function reduce(numer, denomin) {
    var gcd = function gcd(a, b) {
        return b ? gcd(b, a % b) : a;
    };
    gcd = gcd(numer, denomin);
    return [numer / gcd,denomin / gcd];
} 

function simplifyFraction(num, den) {
    var mixed = 0;
    while (num >= den) {
        mixed++;
        num -= den;
    }

    if (num == 0) {
        return mixed;
    }
    var reduced = reduce(num, den);
    // Only a set of denominators can be shown
    // So you don't get amounts like 2/5 Cup
    if (denominators.includes(reduced[1])) {
        if (mixed) {
            return mixed + " " + reduced[0] + "/" + reduced[1];
        }
        return reduced[0] + "/" + reduced[1];
    }
    // Overwise convert to decimal with 2 precision
    var deci = parseFloat(mixed) + parseFloat(reduced[0] / reduced[1]);
    return Math.round((deci + Number.EPSILON) * 100) / 100;
}

function integerFactor(factor, integer) {
    // Even though the amount was an integer, the factor may not be
    // So convert to a fraction and simplify

    //  int * factor 
    var int = parseInt(integer);
    //      int
    //     -----
    //    1/factor
    if (factor < 1) {
        var den = 1 / factor;
        int = simplifyFraction(int,den);
    }
    //  int * factor
    //   ----------
    //       1
    else {
        var num = int * factor;
        int = simplifyFraction(num, 1);
    }
    return int;
}

function decimalFactor(factor, line) {
    // Convert to decimal with 2 precision
    var float = parseFloat(line);
    var newDecimal = Math.round((float * factor + Number.EPSILON) * 100) / 100;
    return newDecimal;
}

function fractionFactor(factor, fractionParts) {
    //fractionParts[0] the whole number
    //fractionParts[1] the numerator
    //fractionParts[2] the denominator

    // Convert to improper fraction
    if (fractionParts[0] != undefined) {
        fractionParts[1] = parseInt(fractionParts[1]) + parseInt(fractionParts[0] * fractionParts[2]);
    }

    if (factor < 1) {
        fractionParts[2] /= factor;
    }
    else {
        fractionParts[1] *= factor;
    }
    return simplifyFraction(fractionParts[1], fractionParts[2]);
}

function numberFactor(factor, line) {
    // Bold modified elements
    var oBold = "<b>";
    var cBold = "</b>";
    var lineParts, newLine;
    // Send the amount to the correct function based on match and stitch the line back together
    if (decimal.test(line)) {
        lineParts = line.match(decimal);
        newLine = oBold + decimalFactor(factor, lineParts[1]) + cBold + lineParts[2];
    }
    else if (fraction.test(line)) {
        lineParts = line.match(fraction);
        var fractionFirst = [lineParts[1]];
        var fractionAll = fractionFirst.concat(lineParts[2].split("/"));
        newLine = oBold + fractionFactor(factor, fractionAll) + cBold + lineParts[3];
    }
    else if (integer.test(line)) {
        lineParts = line.match(integer);
        newLine = oBold + integerFactor(factor, lineParts[1]) + cBold + lineParts[2];
    }
    return newLine;
}

function convertToWeight() {
    var ing = document.getElementById("ingredients").innerHTML;
    // Remove any previous bolded elements
    ing = ing.replace(/<b>/g, "");
    ing = ing.replace(/<\/b>/g, "");
    var factor = document.getElementById("factor").value;
    if (!factor || factor == 1 || factor <= 0) {
        return;
    }
    // Convert factor to number
    if (fraction.test(factor)) {
        var factorParts = factor.split("/");
        factor = factorParts[0] / factorParts[1];
    }

    var li = ing.split("<li>");
    for (var i = 0; i < li.length; ++i) {
        // Only modifying lines that begin with an amount
        if (startsWithDigit.test(li[i])) {

            // If two amounts are present (in a range)
            // Send them each individually and stitch the line back together here
            if (range.test(li[i])) {
                var lineParts = li[i].match(range);
                var first = numberFactor(factor, lineParts[1]);
                var second = numberFactor(factor, lineParts[3]);
                li[i] = first + " " + lineParts[2] + " " + second + lineParts[4];
            }
            else {
                li[i] = numberFactor(factor, li[i]);
            }
        }

    }
    // Replace with new content
    var newIng = li[0];
    for (var i = 1; i < li.length; ++i) {
        newIng += "<li>" + li[i];
    }

    document.getElementById("ingredients").innerHTML = newIng;
}


