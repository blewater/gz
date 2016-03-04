/*!
 * jQuery JavaScript Library v2.1.4
 * http://jquery.com/
 *
 * Includes Sizzle.js
 * http://sizzlejs.com/
 *
 * Copyright 2005, 2014 jQuery Foundation, Inc. and other contributors
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2015-04-28T16:01Z
 */

(function( global, factory ) {

	if ( typeof module === "object" && typeof module.exports === "object" ) {
		// For CommonJS and CommonJS-like environments where a proper `window`
		// is present, execute the factory and get jQuery.
		// For environments that do not have a `window` with a `document`
		// (such as Node.js), expose a factory as module.exports.
		// This accentuates the need for the creation of a real `window`.
		// e.g. var jQuery = require("jquery")(window);
		// See ticket #14549 for more info.
		module.exports = global.document ?
			factory( global, true ) :
			function( w ) {
				if ( !w.document ) {
					throw new Error( "jQuery requires a window with a document" );
				}
				return factory( w );
			};
	} else {
		factory( global );
	}

// Pass this if window is not defined yet
}(typeof window !== "undefined" ? window : this, function( window, noGlobal ) {

// Support: Firefox 18+
// Can't be in strict mode, several libs including ASP.NET trace
// the stack via arguments.caller.callee and Firefox dies if
// you try to trace through "use strict" call chains. (#13335)
//

var arr = [];

var slice = arr.slice;

var concat = arr.concat;

var push = arr.push;

var indexOf = arr.indexOf;

var class2type = {};

var toString = class2type.toString;

var hasOwn = class2type.hasOwnProperty;

var support = {};



var
	// Use the correct document accordingly with window argument (sandbox)
	document = window.document,

	version = "2.1.4",

	// Define a local copy of jQuery
	jQuery = function( selector, context ) {
		// The jQuery object is actually just the init constructor 'enhanced'
		// Need init if jQuery is called (just allow error to be thrown if not included)
		return new jQuery.fn.init( selector, context );
	},

	// Support: Android<4.1
	// Make sure we trim BOM and NBSP
	rtrim = /^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g,

	// Matches dashed string for camelizing
	rmsPrefix = /^-ms-/,
	rdashAlpha = /-([\da-z])/gi,

	// Used by jQuery.camelCase as callback to replace()
	fcamelCase = function( all, letter ) {
		return letter.toUpperCase();
	};

jQuery.fn = jQuery.prototype = {
	// The current version of jQuery being used
	jquery: version,

	constructor: jQuery,

	// Start with an empty selector
	selector: "",

	// The default length of a jQuery object is 0
	length: 0,

	toArray: function() {
		return slice.call( this );
	},

	// Get the Nth element in the matched element set OR
	// Get the whole matched element set as a clean array
	get: function( num ) {
		return num != null ?

			// Return just the one element from the set
			( num < 0 ? this[ num + this.length ] : this[ num ] ) :

			// Return all the elements in a clean array
			slice.call( this );
	},

	// Take an array of elements and push it onto the stack
	// (returning the new matched element set)
	pushStack: function( elems ) {

		// Build a new jQuery matched element set
		var ret = jQuery.merge( this.constructor(), elems );

		// Add the old object onto the stack (as a reference)
		ret.prevObject = this;
		ret.context = this.context;

		// Return the newly-formed element set
		return ret;
	},

	// Execute a callback for every element in the matched set.
	// (You can seed the arguments with an array of args, but this is
	// only used internally.)
	each: function( callback, args ) {
		return jQuery.each( this, callback, args );
	},

	map: function( callback ) {
		return this.pushStack( jQuery.map(this, function( elem, i ) {
			return callback.call( elem, i, elem );
		}));
	},

	slice: function() {
		return this.pushStack( slice.apply( this, arguments ) );
	},

	first: function() {
		return this.eq( 0 );
	},

	last: function() {
		return this.eq( -1 );
	},

	eq: function( i ) {
		var len = this.length,
			j = +i + ( i < 0 ? len : 0 );
		return this.pushStack( j >= 0 && j < len ? [ this[j] ] : [] );
	},

	end: function() {
		return this.prevObject || this.constructor(null);
	},

	// For internal use only.
	// Behaves like an Array's method, not like a jQuery method.
	push: push,
	sort: arr.sort,
	splice: arr.splice
};

jQuery.extend = jQuery.fn.extend = function() {
	var options, name, src, copy, copyIsArray, clone,
		target = arguments[0] || {},
		i = 1,
		length = arguments.length,
		deep = false;

	// Handle a deep copy situation
	if ( typeof target === "boolean" ) {
		deep = target;

		// Skip the boolean and the target
		target = arguments[ i ] || {};
		i++;
	}

	// Handle case when target is a string or something (possible in deep copy)
	if ( typeof target !== "object" && !jQuery.isFunction(target) ) {
		target = {};
	}

	// Extend jQuery itself if only one argument is passed
	if ( i === length ) {
		target = this;
		i--;
	}

	for ( ; i < length; i++ ) {
		// Only deal with non-null/undefined values
		if ( (options = arguments[ i ]) != null ) {
			// Extend the base object
			for ( name in options ) {
				src = target[ name ];
				copy = options[ name ];

				// Prevent never-ending loop
				if ( target === copy ) {
					continue;
				}

				// Recurse if we're merging plain objects or arrays
				if ( deep && copy && ( jQuery.isPlainObject(copy) || (copyIsArray = jQuery.isArray(copy)) ) ) {
					if ( copyIsArray ) {
						copyIsArray = false;
						clone = src && jQuery.isArray(src) ? src : [];

					} else {
						clone = src && jQuery.isPlainObject(src) ? src : {};
					}

					// Never move original objects, clone them
					target[ name ] = jQuery.extend( deep, clone, copy );

				// Don't bring in undefined values
				} else if ( copy !== undefined ) {
					target[ name ] = copy;
				}
			}
		}
	}

	// Return the modified object
	return target;
};

jQuery.extend({
	// Unique for each copy of jQuery on the page
	expando: "jQuery" + ( version + Math.random() ).replace( /\D/g, "" ),

	// Assume jQuery is ready without the ready module
	isReady: true,

	error: function( msg ) {
		throw new Error( msg );
	},

	noop: function() {},

	isFunction: function( obj ) {
		return jQuery.type(obj) === "function";
	},

	isArray: Array.isArray,

	isWindow: function( obj ) {
		return obj != null && obj === obj.window;
	},

	isNumeric: function( obj ) {
		// parseFloat NaNs numeric-cast false positives (null|true|false|"")
		// ...but misinterprets leading-number strings, particularly hex literals ("0x...")
		// subtraction forces infinities to NaN
		// adding 1 corrects loss of precision from parseFloat (#15100)
		return !jQuery.isArray( obj ) && (obj - parseFloat( obj ) + 1) >= 0;
	},

	isPlainObject: function( obj ) {
		// Not plain objects:
		// - Any object or value whose internal [[Class]] property is not "[object Object]"
		// - DOM nodes
		// - window
		if ( jQuery.type( obj ) !== "object" || obj.nodeType || jQuery.isWindow( obj ) ) {
			return false;
		}

		if ( obj.constructor &&
				!hasOwn.call( obj.constructor.prototype, "isPrototypeOf" ) ) {
			return false;
		}

		// If the function hasn't returned already, we're confident that
		// |obj| is a plain object, created by {} or constructed with new Object
		return true;
	},

	isEmptyObject: function( obj ) {
		var name;
		for ( name in obj ) {
			return false;
		}
		return true;
	},

	type: function( obj ) {
		if ( obj == null ) {
			return obj + "";
		}
		// Support: Android<4.0, iOS<6 (functionish RegExp)
		return typeof obj === "object" || typeof obj === "function" ?
			class2type[ toString.call(obj) ] || "object" :
			typeof obj;
	},

	// Evaluates a script in a global context
	globalEval: function( code ) {
		var script,
			indirect = eval;

		code = jQuery.trim( code );

		if ( code ) {
			// If the code includes a valid, prologue position
			// strict mode pragma, execute code by injecting a
			// script tag into the document.
			if ( code.indexOf("use strict") === 1 ) {
				script = document.createElement("script");
				script.text = code;
				document.head.appendChild( script ).parentNode.removeChild( script );
			} else {
			// Otherwise, avoid the DOM node creation, insertion
			// and removal by using an indirect global eval
				indirect( code );
			}
		}
	},

	// Convert dashed to camelCase; used by the css and data modules
	// Support: IE9-11+
	// Microsoft forgot to hump their vendor prefix (#9572)
	camelCase: function( string ) {
		return string.replace( rmsPrefix, "ms-" ).replace( rdashAlpha, fcamelCase );
	},

	nodeName: function( elem, name ) {
		return elem.nodeName && elem.nodeName.toLowerCase() === name.toLowerCase();
	},

	// args is for internal usage only
	each: function( obj, callback, args ) {
		var value,
			i = 0,
			length = obj.length,
			isArray = isArraylike( obj );

		if ( args ) {
			if ( isArray ) {
				for ( ; i < length; i++ ) {
					value = callback.apply( obj[ i ], args );

					if ( value === false ) {
						break;
					}
				}
			} else {
				for ( i in obj ) {
					value = callback.apply( obj[ i ], args );

					if ( value === false ) {
						break;
					}
				}
			}

		// A special, fast, case for the most common use of each
		} else {
			if ( isArray ) {
				for ( ; i < length; i++ ) {
					value = callback.call( obj[ i ], i, obj[ i ] );

					if ( value === false ) {
						break;
					}
				}
			} else {
				for ( i in obj ) {
					value = callback.call( obj[ i ], i, obj[ i ] );

					if ( value === false ) {
						break;
					}
				}
			}
		}

		return obj;
	},

	// Support: Android<4.1
	trim: function( text ) {
		return text == null ?
			"" :
			( text + "" ).replace( rtrim, "" );
	},

	// results is for internal usage only
	makeArray: function( arr, results ) {
		var ret = results || [];

		if ( arr != null ) {
			if ( isArraylike( Object(arr) ) ) {
				jQuery.merge( ret,
					typeof arr === "string" ?
					[ arr ] : arr
				);
			} else {
				push.call( ret, arr );
			}
		}

		return ret;
	},

	inArray: function( elem, arr, i ) {
		return arr == null ? -1 : indexOf.call( arr, elem, i );
	},

	merge: function( first, second ) {
		var len = +second.length,
			j = 0,
			i = first.length;

		for ( ; j < len; j++ ) {
			first[ i++ ] = second[ j ];
		}

		first.length = i;

		return first;
	},

	grep: function( elems, callback, invert ) {
		var callbackInverse,
			matches = [],
			i = 0,
			length = elems.length,
			callbackExpect = !invert;

		// Go through the array, only saving the items
		// that pass the validator function
		for ( ; i < length; i++ ) {
			callbackInverse = !callback( elems[ i ], i );
			if ( callbackInverse !== callbackExpect ) {
				matches.push( elems[ i ] );
			}
		}

		return matches;
	},

	// arg is for internal usage only
	map: function( elems, callback, arg ) {
		var value,
			i = 0,
			length = elems.length,
			isArray = isArraylike( elems ),
			ret = [];

		// Go through the array, translating each of the items to their new values
		if ( isArray ) {
			for ( ; i < length; i++ ) {
				value = callback( elems[ i ], i, arg );

				if ( value != null ) {
					ret.push( value );
				}
			}

		// Go through every key on the object,
		} else {
			for ( i in elems ) {
				value = callback( elems[ i ], i, arg );

				if ( value != null ) {
					ret.push( value );
				}
			}
		}

		// Flatten any nested arrays
		return concat.apply( [], ret );
	},

	// A global GUID counter for objects
	guid: 1,

	// Bind a function to a context, optionally partially applying any
	// arguments.
	proxy: function( fn, context ) {
		var tmp, args, proxy;

		if ( typeof context === "string" ) {
			tmp = fn[ context ];
			context = fn;
			fn = tmp;
		}

		// Quick check to determine if target is callable, in the spec
		// this throws a TypeError, but we will just return undefined.
		if ( !jQuery.isFunction( fn ) ) {
			return undefined;
		}

		// Simulated bind
		args = slice.call( arguments, 2 );
		proxy = function() {
			return fn.apply( context || this, args.concat( slice.call( arguments ) ) );
		};

		// Set the guid of unique handler to the same of original handler, so it can be removed
		proxy.guid = fn.guid = fn.guid || jQuery.guid++;

		return proxy;
	},

	now: Date.now,

	// jQuery.support is not used in Core but other projects attach their
	// properties to it so it needs to exist.
	support: support
});

// Populate the class2type map
jQuery.each("Boolean Number String Function Array Date RegExp Object Error".split(" "), function(i, name) {
	class2type[ "[object " + name + "]" ] = name.toLowerCase();
});

function isArraylike( obj ) {

	// Support: iOS 8.2 (not reproducible in simulator)
	// `in` check used to prevent JIT error (gh-2145)
	// hasOwn isn't used here due to false negatives
	// regarding Nodelist length in IE
	var length = "length" in obj && obj.length,
		type = jQuery.type( obj );

	if ( type === "function" || jQuery.isWindow( obj ) ) {
		return false;
	}

	if ( obj.nodeType === 1 && length ) {
		return true;
	}

	return type === "array" || length === 0 ||
		typeof length === "number" && length > 0 && ( length - 1 ) in obj;
}
var Sizzle =
/*!
 * Sizzle CSS Selector Engine v2.2.0-pre
 * http://sizzlejs.com/
 *
 * Copyright 2008, 2014 jQuery Foundation, Inc. and other contributors
 * Released under the MIT license
 * http://jquery.org/license
 *
 * Date: 2014-12-16
 */
(function( window ) {

var i,
	support,
	Expr,
	getText,
	isXML,
	tokenize,
	compile,
	select,
	outermostContext,
	sortInput,
	hasDuplicate,

	// Local document vars
	setDocument,
	document,
	docElem,
	documentIsHTML,
	rbuggyQSA,
	rbuggyMatches,
	matches,
	contains,

	// Instance-specific data
	expando = "sizzle" + 1 * new Date(),
	preferredDoc = window.document,
	dirruns = 0,
	done = 0,
	classCache = createCache(),
	tokenCache = createCache(),
	compilerCache = createCache(),
	sortOrder = function( a, b ) {
		if ( a === b ) {
			hasDuplicate = true;
		}
		return 0;
	},

	// General-purpose constants
	MAX_NEGATIVE = 1 << 31,

	// Instance methods
	hasOwn = ({}).hasOwnProperty,
	arr = [],
	pop = arr.pop,
	push_native = arr.push,
	push = arr.push,
	slice = arr.slice,
	// Use a stripped-down indexOf as it's faster than native
	// http://jsperf.com/thor-indexof-vs-for/5
	indexOf = function( list, elem ) {
		var i = 0,
			len = list.length;
		for ( ; i < len; i++ ) {
			if ( list[i] === elem ) {
				return i;
			}
		}
		return -1;
	},

	booleans = "checked|selected|async|autofocus|autoplay|controls|defer|disabled|hidden|ismap|loop|multiple|open|readonly|required|scoped",

	// Regular expressions

	// Whitespace characters http://www.w3.org/TR/css3-selectors/#whitespace
	whitespace = "[\\x20\\t\\r\\n\\f]",
	// http://www.w3.org/TR/css3-syntax/#characters
	characterEncoding = "(?:\\\\.|[\\w-]|[^\\x00-\\xa0])+",

	// Loosely modeled on CSS identifier characters
	// An unquoted value should be a CSS identifier http://www.w3.org/TR/css3-selectors/#attribute-selectors
	// Proper syntax: http://www.w3.org/TR/CSS21/syndata.html#value-def-identifier
	identifier = characterEncoding.replace( "w", "w#" ),

	// Attribute selectors: http://www.w3.org/TR/selectors/#attribute-selectors
	attributes = "\\[" + whitespace + "*(" + characterEncoding + ")(?:" + whitespace +
		// Operator (capture 2)
		"*([*^$|!~]?=)" + whitespace +
		// "Attribute values must be CSS identifiers [capture 5] or strings [capture 3 or capture 4]"
		"*(?:'((?:\\\\.|[^\\\\'])*)'|\"((?:\\\\.|[^\\\\\"])*)\"|(" + identifier + "))|)" + whitespace +
		"*\\]",

	pseudos = ":(" + characterEncoding + ")(?:\\((" +
		// To reduce the number of selectors needing tokenize in the preFilter, prefer arguments:
		// 1. quoted (capture 3; capture 4 or capture 5)
		"('((?:\\\\.|[^\\\\'])*)'|\"((?:\\\\.|[^\\\\\"])*)\")|" +
		// 2. simple (capture 6)
		"((?:\\\\.|[^\\\\()[\\]]|" + attributes + ")*)|" +
		// 3. anything else (capture 2)
		".*" +
		")\\)|)",

	// Leading and non-escaped trailing whitespace, capturing some non-whitespace characters preceding the latter
	rwhitespace = new RegExp( whitespace + "+", "g" ),
	rtrim = new RegExp( "^" + whitespace + "+|((?:^|[^\\\\])(?:\\\\.)*)" + whitespace + "+$", "g" ),

	rcomma = new RegExp( "^" + whitespace + "*," + whitespace + "*" ),
	rcombinators = new RegExp( "^" + whitespace + "*([>+~]|" + whitespace + ")" + whitespace + "*" ),

	rattributeQuotes = new RegExp( "=" + whitespace + "*([^\\]'\"]*?)" + whitespace + "*\\]", "g" ),

	rpseudo = new RegExp( pseudos ),
	ridentifier = new RegExp( "^" + identifier + "$" ),

	matchExpr = {
		"ID": new RegExp( "^#(" + characterEncoding + ")" ),
		"CLASS": new RegExp( "^\\.(" + characterEncoding + ")" ),
		"TAG": new RegExp( "^(" + characterEncoding.replace( "w", "w*" ) + ")" ),
		"ATTR": new RegExp( "^" + attributes ),
		"PSEUDO": new RegExp( "^" + pseudos ),
		"CHILD": new RegExp( "^:(only|first|last|nth|nth-last)-(child|of-type)(?:\\(" + whitespace +
			"*(even|odd|(([+-]|)(\\d*)n|)" + whitespace + "*(?:([+-]|)" + whitespace +
			"*(\\d+)|))" + whitespace + "*\\)|)", "i" ),
		"bool": new RegExp( "^(?:" + booleans + ")$", "i" ),
		// For use in libraries implementing .is()
		// We use this for POS matching in `select`
		"needsContext": new RegExp( "^" + whitespace + "*[>+~]|:(even|odd|eq|gt|lt|nth|first|last)(?:\\(" +
			whitespace + "*((?:-\\d)?\\d*)" + whitespace + "*\\)|)(?=[^-]|$)", "i" )
	},

	rinputs = /^(?:input|select|textarea|button)$/i,
	rheader = /^h\d$/i,

	rnative = /^[^{]+\{\s*\[native \w/,

	// Easily-parseable/retrievable ID or TAG or CLASS selectors
	rquickExpr = /^(?:#([\w-]+)|(\w+)|\.([\w-]+))$/,

	rsibling = /[+~]/,
	rescape = /'|\\/g,

	// CSS escapes http://www.w3.org/TR/CSS21/syndata.html#escaped-characters
	runescape = new RegExp( "\\\\([\\da-f]{1,6}" + whitespace + "?|(" + whitespace + ")|.)", "ig" ),
	funescape = function( _, escaped, escapedWhitespace ) {
		var high = "0x" + escaped - 0x10000;
		// NaN means non-codepoint
		// Support: Firefox<24
		// Workaround erroneous numeric interpretation of +"0x"
		return high !== high || escapedWhitespace ?
			escaped :
			high < 0 ?
				// BMP codepoint
				String.fromCharCode( high + 0x10000 ) :
				// Supplemental Plane codepoint (surrogate pair)
				String.fromCharCode( high >> 10 | 0xD800, high & 0x3FF | 0xDC00 );
	},

	// Used for iframes
	// See setDocument()
	// Removing the function wrapper causes a "Permission Denied"
	// error in IE
	unloadHandler = function() {
		setDocument();
	};

// Optimize for push.apply( _, NodeList )
try {
	push.apply(
		(arr = slice.call( preferredDoc.childNodes )),
		preferredDoc.childNodes
	);
	// Support: Android<4.0
	// Detect silently failing push.apply
	arr[ preferredDoc.childNodes.length ].nodeType;
} catch ( e ) {
	push = { apply: arr.length ?

		// Leverage slice if possible
		function( target, els ) {
			push_native.apply( target, slice.call(els) );
		} :

		// Support: IE<9
		// Otherwise append directly
		function( target, els ) {
			var j = target.length,
				i = 0;
			// Can't trust NodeList.length
			while ( (target[j++] = els[i++]) ) {}
			target.length = j - 1;
		}
	};
}

function Sizzle( selector, context, results, seed ) {
	var match, elem, m, nodeType,
		// QSA vars
		i, groups, old, nid, newContext, newSelector;

	if ( ( context ? context.ownerDocument || context : preferredDoc ) !== document ) {
		setDocument( context );
	}

	context = context || document;
	results = results || [];
	nodeType = context.nodeType;

	if ( typeof selector !== "string" || !selector ||
		nodeType !== 1 && nodeType !== 9 && nodeType !== 11 ) {

		return results;
	}

	if ( !seed && documentIsHTML ) {

		// Try to shortcut find operations when possible (e.g., not under DocumentFragment)
		if ( nodeType !== 11 && (match = rquickExpr.exec( selector )) ) {
			// Speed-up: Sizzle("#ID")
			if ( (m = match[1]) ) {
				if ( nodeType === 9 ) {
					elem = context.getElementById( m );
					// Check parentNode to catch when Blackberry 4.6 returns
					// nodes that are no longer in the document (jQuery #6963)
					if ( elem && elem.parentNode ) {
						// Handle the case where IE, Opera, and Webkit return items
						// by name instead of ID
						if ( elem.id === m ) {
							results.push( elem );
							return results;
						}
					} else {
						return results;
					}
				} else {
					// Context is not a document
					if ( context.ownerDocument && (elem = context.ownerDocument.getElementById( m )) &&
						contains( context, elem ) && elem.id === m ) {
						results.push( elem );
						return results;
					}
				}

			// Speed-up: Sizzle("TAG")
			} else if ( match[2] ) {
				push.apply( results, context.getElementsByTagName( selector ) );
				return results;

			// Speed-up: Sizzle(".CLASS")
			} else if ( (m = match[3]) && support.getElementsByClassName ) {
				push.apply( results, context.getElementsByClassName( m ) );
				return results;
			}
		}

		// QSA path
		if ( support.qsa && (!rbuggyQSA || !rbuggyQSA.test( selector )) ) {
			nid = old = expando;
			newContext = context;
			newSelector = nodeType !== 1 && selector;

			// qSA works strangely on Element-rooted queries
			// We can work around this by specifying an extra ID on the root
			// and working up from there (Thanks to Andrew Dupont for the technique)
			// IE 8 doesn't work on object elements
			if ( nodeType === 1 && context.nodeName.toLowerCase() !== "object" ) {
				groups = tokenize( selector );

				if ( (old = context.getAttribute("id")) ) {
					nid = old.replace( rescape, "\\$&" );
				} else {
					context.setAttribute( "id", nid );
				}
				nid = "[id='" + nid + "'] ";

				i = groups.length;
				while ( i-- ) {
					groups[i] = nid + toSelector( groups[i] );
				}
				newContext = rsibling.test( selector ) && testContext( context.parentNode ) || context;
				newSelector = groups.join(",");
			}

			if ( newSelector ) {
				try {
					push.apply( results,
						newContext.querySelectorAll( newSelector )
					);
					return results;
				} catch(qsaError) {
				} finally {
					if ( !old ) {
						context.removeAttribute("id");
					}
				}
			}
		}
	}

	// All others
	return select( selector.replace( rtrim, "$1" ), context, results, seed );
}

/**
 * Create key-value caches of limited size
 * @returns {Function(string, Object)} Returns the Object data after storing it on itself with
 *	property name the (space-suffixed) string and (if the cache is larger than Expr.cacheLength)
 *	deleting the oldest entry
 */
function createCache() {
	var keys = [];

	function cache( key, value ) {
		// Use (key + " ") to avoid collision with native prototype properties (see Issue #157)
		if ( keys.push( key + " " ) > Expr.cacheLength ) {
			// Only keep the most recent entries
			delete cache[ keys.shift() ];
		}
		return (cache[ key + " " ] = value);
	}
	return cache;
}

/**
 * Mark a function for special use by Sizzle
 * @param {Function} fn The function to mark
 */
function markFunction( fn ) {
	fn[ expando ] = true;
	return fn;
}

/**
 * Support testing using an element
 * @param {Function} fn Passed the created div and expects a boolean result
 */
function assert( fn ) {
	var div = document.createElement("div");

	try {
		return !!fn( div );
	} catch (e) {
		return false;
	} finally {
		// Remove from its parent by default
		if ( div.parentNode ) {
			div.parentNode.removeChild( div );
		}
		// release memory in IE
		div = null;
	}
}

/**
 * Adds the same handler for all of the specified attrs
 * @param {String} attrs Pipe-separated list of attributes
 * @param {Function} handler The method that will be applied
 */
function addHandle( attrs, handler ) {
	var arr = attrs.split("|"),
		i = attrs.length;

	while ( i-- ) {
		Expr.attrHandle[ arr[i] ] = handler;
	}
}

/**
 * Checks document order of two siblings
 * @param {Element} a
 * @param {Element} b
 * @returns {Number} Returns less than 0 if a precedes b, greater than 0 if a follows b
 */
function siblingCheck( a, b ) {
	var cur = b && a,
		diff = cur && a.nodeType === 1 && b.nodeType === 1 &&
			( ~b.sourceIndex || MAX_NEGATIVE ) -
			( ~a.sourceIndex || MAX_NEGATIVE );

	// Use IE sourceIndex if available on both nodes
	if ( diff ) {
		return diff;
	}

	// Check if b follows a
	if ( cur ) {
		while ( (cur = cur.nextSibling) ) {
			if ( cur === b ) {
				return -1;
			}
		}
	}

	return a ? 1 : -1;
}

/**
 * Returns a function to use in pseudos for input types
 * @param {String} type
 */
function createInputPseudo( type ) {
	return function( elem ) {
		var name = elem.nodeName.toLowerCase();
		return name === "input" && elem.type === type;
	};
}

/**
 * Returns a function to use in pseudos for buttons
 * @param {String} type
 */
function createButtonPseudo( type ) {
	return function( elem ) {
		var name = elem.nodeName.toLowerCase();
		return (name === "input" || name === "button") && elem.type === type;
	};
}

/**
 * Returns a function to use in pseudos for positionals
 * @param {Function} fn
 */
function createPositionalPseudo( fn ) {
	return markFunction(function( argument ) {
		argument = +argument;
		return markFunction(function( seed, matches ) {
			var j,
				matchIndexes = fn( [], seed.length, argument ),
				i = matchIndexes.length;

			// Match elements found at the specified indexes
			while ( i-- ) {
				if ( seed[ (j = matchIndexes[i]) ] ) {
					seed[j] = !(matches[j] = seed[j]);
				}
			}
		});
	});
}

/**
 * Checks a node for validity as a Sizzle context
 * @param {Element|Object=} context
 * @returns {Element|Object|Boolean} The input node if acceptable, otherwise a falsy value
 */
function testContext( context ) {
	return context && typeof context.getElementsByTagName !== "undefined" && context;
}

// Expose support vars for convenience
support = Sizzle.support = {};

/**
 * Detects XML nodes
 * @param {Element|Object} elem An element or a document
 * @returns {Boolean} True iff elem is a non-HTML XML node
 */
isXML = Sizzle.isXML = function( elem ) {
	// documentElement is verified for cases where it doesn't yet exist
	// (such as loading iframes in IE - #4833)
	var documentElement = elem && (elem.ownerDocument || elem).documentElement;
	return documentElement ? documentElement.nodeName !== "HTML" : false;
};

/**
 * Sets document-related variables once based on the current document
 * @param {Element|Object} [doc] An element or document object to use to set the document
 * @returns {Object} Returns the current document
 */
setDocument = Sizzle.setDocument = function( node ) {
	var hasCompare, parent,
		doc = node ? node.ownerDocument || node : preferredDoc;

	// If no document and documentElement is available, return
	if ( doc === document || doc.nodeType !== 9 || !doc.documentElement ) {
		return document;
	}

	// Set our document
	document = doc;
	docElem = doc.documentElement;
	parent = doc.defaultView;

	// Support: IE>8
	// If iframe document is assigned to "document" variable and if iframe has been reloaded,
	// IE will throw "permission denied" error when accessing "document" variable, see jQuery #13936
	// IE6-8 do not support the defaultView property so parent will be undefined
	if ( parent && parent !== parent.top ) {
		// IE11 does not have attachEvent, so all must suffer
		if ( parent.addEventListener ) {
			parent.addEventListener( "unload", unloadHandler, false );
		} else if ( parent.attachEvent ) {
			parent.attachEvent( "onunload", unloadHandler );
		}
	}

	/* Support tests
	---------------------------------------------------------------------- */
	documentIsHTML = !isXML( doc );

	/* Attributes
	---------------------------------------------------------------------- */

	// Support: IE<8
	// Verify that getAttribute really returns attributes and not properties
	// (excepting IE8 booleans)
	support.attributes = assert(function( div ) {
		div.className = "i";
		return !div.getAttribute("className");
	});

	/* getElement(s)By*
	---------------------------------------------------------------------- */

	// Check if getElementsByTagName("*") returns only elements
	support.getElementsByTagName = assert(function( div ) {
		div.appendChild( doc.createComment("") );
		return !div.getElementsByTagName("*").length;
	});

	// Support: IE<9
	support.getElementsByClassName = rnative.test( doc.getElementsByClassName );

	// Support: IE<10
	// Check if getElementById returns elements by name
	// The broken getElementById methods don't pick up programatically-set names,
	// so use a roundabout getElementsByName test
	support.getById = assert(function( div ) {
		docElem.appendChild( div ).id = expando;
		return !doc.getElementsByName || !doc.getElementsByName( expando ).length;
	});

	// ID find and filter
	if ( support.getById ) {
		Expr.find["ID"] = function( id, context ) {
			if ( typeof context.getElementById !== "undefined" && documentIsHTML ) {
				var m = context.getElementById( id );
				// Check parentNode to catch when Blackberry 4.6 returns
				// nodes that are no longer in the document #6963
				return m && m.parentNode ? [ m ] : [];
			}
		};
		Expr.filter["ID"] = function( id ) {
			var attrId = id.replace( runescape, funescape );
			return function( elem ) {
				return elem.getAttribute("id") === attrId;
			};
		};
	} else {
		// Support: IE6/7
		// getElementById is not reliable as a find shortcut
		delete Expr.find["ID"];

		Expr.filter["ID"] =  function( id ) {
			var attrId = id.replace( runescape, funescape );
			return function( elem ) {
				var node = typeof elem.getAttributeNode !== "undefined" && elem.getAttributeNode("id");
				return node && node.value === attrId;
			};
		};
	}

	// Tag
	Expr.find["TAG"] = support.getElementsByTagName ?
		function( tag, context ) {
			if ( typeof context.getElementsByTagName !== "undefined" ) {
				return context.getElementsByTagName( tag );

			// DocumentFragment nodes don't have gEBTN
			} else if ( support.qsa ) {
				return context.querySelectorAll( tag );
			}
		} :

		function( tag, context ) {
			var elem,
				tmp = [],
				i = 0,
				// By happy coincidence, a (broken) gEBTN appears on DocumentFragment nodes too
				results = context.getElementsByTagName( tag );

			// Filter out possible comments
			if ( tag === "*" ) {
				while ( (elem = results[i++]) ) {
					if ( elem.nodeType === 1 ) {
						tmp.push( elem );
					}
				}

				return tmp;
			}
			return results;
		};

	// Class
	Expr.find["CLASS"] = support.getElementsByClassName && function( className, context ) {
		if ( documentIsHTML ) {
			return context.getElementsByClassName( className );
		}
	};

	/* QSA/matchesSelector
	---------------------------------------------------------------------- */

	// QSA and matchesSelector support

	// matchesSelector(:active) reports false when true (IE9/Opera 11.5)
	rbuggyMatches = [];

	// qSa(:focus) reports false when true (Chrome 21)
	// We allow this because of a bug in IE8/9 that throws an error
	// whenever `document.activeElement` is accessed on an iframe
	// So, we allow :focus to pass through QSA all the time to avoid the IE error
	// See http://bugs.jquery.com/ticket/13378
	rbuggyQSA = [];

	if ( (support.qsa = rnative.test( doc.querySelectorAll )) ) {
		// Build QSA regex
		// Regex strategy adopted from Diego Perini
		assert(function( div ) {
			// Select is set to empty string on purpose
			// This is to test IE's treatment of not explicitly
			// setting a boolean content attribute,
			// since its presence should be enough
			// http://bugs.jquery.com/ticket/12359
			docElem.appendChild( div ).innerHTML = "<a id='" + expando + "'></a>" +
				"<select id='" + expando + "-\f]' msallowcapture=''>" +
				"<option selected=''></option></select>";

			// Support: IE8, Opera 11-12.16
			// Nothing should be selected when empty strings follow ^= or $= or *=
			// The test attribute must be unknown in Opera but "safe" for WinRT
			// http://msdn.microsoft.com/en-us/library/ie/hh465388.aspx#attribute_section
			if ( div.querySelectorAll("[msallowcapture^='']").length ) {
				rbuggyQSA.push( "[*^$]=" + whitespace + "*(?:''|\"\")" );
			}

			// Support: IE8
			// Boolean attributes and "value" are not treated correctly
			if ( !div.querySelectorAll("[selected]").length ) {
				rbuggyQSA.push( "\\[" + whitespace + "*(?:value|" + booleans + ")" );
			}

			// Support: Chrome<29, Android<4.2+, Safari<7.0+, iOS<7.0+, PhantomJS<1.9.7+
			if ( !div.querySelectorAll( "[id~=" + expando + "-]" ).length ) {
				rbuggyQSA.push("~=");
			}

			// Webkit/Opera - :checked should return selected option elements
			// http://www.w3.org/TR/2011/REC-css3-selectors-20110929/#checked
			// IE8 throws error here and will not see later tests
			if ( !div.querySelectorAll(":checked").length ) {
				rbuggyQSA.push(":checked");
			}

			// Support: Safari 8+, iOS 8+
			// https://bugs.webkit.org/show_bug.cgi?id=136851
			// In-page `selector#id sibing-combinator selector` fails
			if ( !div.querySelectorAll( "a#" + expando + "+*" ).length ) {
				rbuggyQSA.push(".#.+[+~]");
			}
		});

		assert(function( div ) {
			// Support: Windows 8 Native Apps
			// The type and name attributes are restricted during .innerHTML assignment
			var input = doc.createElement("input");
			input.setAttribute( "type", "hidden" );
			div.appendChild( input ).setAttribute( "name", "D" );

			// Support: IE8
			// Enforce case-sensitivity of name attribute
			if ( div.querySelectorAll("[name=d]").length ) {
				rbuggyQSA.push( "name" + whitespace + "*[*^$|!~]?=" );
			}

			// FF 3.5 - :enabled/:disabled and hidden elements (hidden elements are still enabled)
			// IE8 throws error here and will not see later tests
			if ( !div.querySelectorAll(":enabled").length ) {
				rbuggyQSA.push( ":enabled", ":disabled" );
			}

			// Opera 10-11 does not throw on post-comma invalid pseudos
			div.querySelectorAll("*,:x");
			rbuggyQSA.push(",.*:");
		});
	}

	if ( (support.matchesSelector = rnative.test( (matches = docElem.matches ||
		docElem.webkitMatchesSelector ||
		docElem.mozMatchesSelector ||
		docElem.oMatchesSelector ||
		docElem.msMatchesSelector) )) ) {

		assert(function( div ) {
			// Check to see if it's possible to do matchesSelector
			// on a disconnected node (IE 9)
			support.disconnectedMatch = matches.call( div, "div" );

			// This should fail with an exception
			// Gecko does not error, returns false instead
			matches.call( div, "[s!='']:x" );
			rbuggyMatches.push( "!=", pseudos );
		});
	}

	rbuggyQSA = rbuggyQSA.length && new RegExp( rbuggyQSA.join("|") );
	rbuggyMatches = rbuggyMatches.length && new RegExp( rbuggyMatches.join("|") );

	/* Contains
	---------------------------------------------------------------------- */
	hasCompare = rnative.test( docElem.compareDocumentPosition );

	// Element contains another
	// Purposefully does not implement inclusive descendent
	// As in, an element does not contain itself
	contains = hasCompare || rnative.test( docElem.contains ) ?
		function( a, b ) {
			var adown = a.nodeType === 9 ? a.documentElement : a,
				bup = b && b.parentNode;
			return a === bup || !!( bup && bup.nodeType === 1 && (
				adown.contains ?
					adown.contains( bup ) :
					a.compareDocumentPosition && a.compareDocumentPosition( bup ) & 16
			));
		} :
		function( a, b ) {
			if ( b ) {
				while ( (b = b.parentNode) ) {
					if ( b === a ) {
						return true;
					}
				}
			}
			return false;
		};

	/* Sorting
	---------------------------------------------------------------------- */

	// Document order sorting
	sortOrder = hasCompare ?
	function( a, b ) {

		// Flag for duplicate removal
		if ( a === b ) {
			hasDuplicate = true;
			return 0;
		}

		// Sort on method existence if only one input has compareDocumentPosition
		var compare = !a.compareDocumentPosition - !b.compareDocumentPosition;
		if ( compare ) {
			return compare;
		}

		// Calculate position if both inputs belong to the same document
		compare = ( a.ownerDocument || a ) === ( b.ownerDocument || b ) ?
			a.compareDocumentPosition( b ) :

			// Otherwise we know they are disconnected
			1;

		// Disconnected nodes
		if ( compare & 1 ||
			(!support.sortDetached && b.compareDocumentPosition( a ) === compare) ) {

			// Choose the first element that is related to our preferred document
			if ( a === doc || a.ownerDocument === preferredDoc && contains(preferredDoc, a) ) {
				return -1;
			}
			if ( b === doc || b.ownerDocument === preferredDoc && contains(preferredDoc, b) ) {
				return 1;
			}

			// Maintain original order
			return sortInput ?
				( indexOf( sortInput, a ) - indexOf( sortInput, b ) ) :
				0;
		}

		return compare & 4 ? -1 : 1;
	} :
	function( a, b ) {
		// Exit early if the nodes are identical
		if ( a === b ) {
			hasDuplicate = true;
			return 0;
		}

		var cur,
			i = 0,
			aup = a.parentNode,
			bup = b.parentNode,
			ap = [ a ],
			bp = [ b ];

		// Parentless nodes are either documents or disconnected
		if ( !aup || !bup ) {
			return a === doc ? -1 :
				b === doc ? 1 :
				aup ? -1 :
				bup ? 1 :
				sortInput ?
				( indexOf( sortInput, a ) - indexOf( sortInput, b ) ) :
				0;

		// If the nodes are siblings, we can do a quick check
		} else if ( aup === bup ) {
			return siblingCheck( a, b );
		}

		// Otherwise we need full lists of their ancestors for comparison
		cur = a;
		while ( (cur = cur.parentNode) ) {
			ap.unshift( cur );
		}
		cur = b;
		while ( (cur = cur.parentNode) ) {
			bp.unshift( cur );
		}

		// Walk down the tree looking for a discrepancy
		while ( ap[i] === bp[i] ) {
			i++;
		}

		return i ?
			// Do a sibling check if the nodes have a common ancestor
			siblingCheck( ap[i], bp[i] ) :

			// Otherwise nodes in our document sort first
			ap[i] === preferredDoc ? -1 :
			bp[i] === preferredDoc ? 1 :
			0;
	};

	return doc;
};

Sizzle.matches = function( expr, elements ) {
	return Sizzle( expr, null, null, elements );
};

Sizzle.matchesSelector = function( elem, expr ) {
	// Set document vars if needed
	if ( ( elem.ownerDocument || elem ) !== document ) {
		setDocument( elem );
	}

	// Make sure that attribute selectors are quoted
	expr = expr.replace( rattributeQuotes, "='$1']" );

	if ( support.matchesSelector && documentIsHTML &&
		( !rbuggyMatches || !rbuggyMatches.test( expr ) ) &&
		( !rbuggyQSA     || !rbuggyQSA.test( expr ) ) ) {

		try {
			var ret = matches.call( elem, expr );

			// IE 9's matchesSelector returns false on disconnected nodes
			if ( ret || support.disconnectedMatch ||
					// As well, disconnected nodes are said to be in a document
					// fragment in IE 9
					elem.document && elem.document.nodeType !== 11 ) {
				return ret;
			}
		} catch (e) {}
	}

	return Sizzle( expr, document, null, [ elem ] ).length > 0;
};

Sizzle.contains = function( context, elem ) {
	// Set document vars if needed
	if ( ( context.ownerDocument || context ) !== document ) {
		setDocument( context );
	}
	return contains( context, elem );
};

Sizzle.attr = function( elem, name ) {
	// Set document vars if needed
	if ( ( elem.ownerDocument || elem ) !== document ) {
		setDocument( elem );
	}

	var fn = Expr.attrHandle[ name.toLowerCase() ],
		// Don't get fooled by Object.prototype properties (jQuery #13807)
		val = fn && hasOwn.call( Expr.attrHandle, name.toLowerCase() ) ?
			fn( elem, name, !documentIsHTML ) :
			undefined;

	return val !== undefined ?
		val :
		support.attributes || !documentIsHTML ?
			elem.getAttribute( name ) :
			(val = elem.getAttributeNode(name)) && val.specified ?
				val.value :
				null;
};

Sizzle.error = function( msg ) {
	throw new Error( "Syntax error, unrecognized expression: " + msg );
};

/**
 * Document sorting and removing duplicates
 * @param {ArrayLike} results
 */
Sizzle.uniqueSort = function( results ) {
	var elem,
		duplicates = [],
		j = 0,
		i = 0;

	// Unless we *know* we can detect duplicates, assume their presence
	hasDuplicate = !support.detectDuplicates;
	sortInput = !support.sortStable && results.slice( 0 );
	results.sort( sortOrder );

	if ( hasDuplicate ) {
		while ( (elem = results[i++]) ) {
			if ( elem === results[ i ] ) {
				j = duplicates.push( i );
			}
		}
		while ( j-- ) {
			results.splice( duplicates[ j ], 1 );
		}
	}

	// Clear input after sorting to release objects
	// See https://github.com/jquery/sizzle/pull/225
	sortInput = null;

	return results;
};

/**
 * Utility function for retrieving the text value of an array of DOM nodes
 * @param {Array|Element} elem
 */
getText = Sizzle.getText = function( elem ) {
	var node,
		ret = "",
		i = 0,
		nodeType = elem.nodeType;

	if ( !nodeType ) {
		// If no nodeType, this is expected to be an array
		while ( (node = elem[i++]) ) {
			// Do not traverse comment nodes
			ret += getText( node );
		}
	} else if ( nodeType === 1 || nodeType === 9 || nodeType === 11 ) {
		// Use textContent for elements
		// innerText usage removed for consistency of new lines (jQuery #11153)
		if ( typeof elem.textContent === "string" ) {
			return elem.textContent;
		} else {
			// Traverse its children
			for ( elem = elem.firstChild; elem; elem = elem.nextSibling ) {
				ret += getText( elem );
			}
		}
	} else if ( nodeType === 3 || nodeType === 4 ) {
		return elem.nodeValue;
	}
	// Do not include comment or processing instruction nodes

	return ret;
};

Expr = Sizzle.selectors = {

	// Can be adjusted by the user
	cacheLength: 50,

	createPseudo: markFunction,

	match: matchExpr,

	attrHandle: {},

	find: {},

	relative: {
		">": { dir: "parentNode", first: true },
		" ": { dir: "parentNode" },
		"+": { dir: "previousSibling", first: true },
		"~": { dir: "previousSibling" }
	},

	preFilter: {
		"ATTR": function( match ) {
			match[1] = match[1].replace( runescape, funescape );

			// Move the given value to match[3] whether quoted or unquoted
			match[3] = ( match[3] || match[4] || match[5] || "" ).replace( runescape, funescape );

			if ( match[2] === "~=" ) {
				match[3] = " " + match[3] + " ";
			}

			return match.slice( 0, 4 );
		},

		"CHILD": function( match ) {
			/* matches from matchExpr["CHILD"]
				1 type (only|nth|...)
				2 what (child|of-type)
				3 argument (even|odd|\d*|\d*n([+-]\d+)?|...)
				4 xn-component of xn+y argument ([+-]?\d*n|)
				5 sign of xn-component
				6 x of xn-component
				7 sign of y-component
				8 y of y-component
			*/
			match[1] = match[1].toLowerCase();

			if ( match[1].slice( 0, 3 ) === "nth" ) {
				// nth-* requires argument
				if ( !match[3] ) {
					Sizzle.error( match[0] );
				}

				// numeric x and y parameters for Expr.filter.CHILD
				// remember that false/true cast respectively to 0/1
				match[4] = +( match[4] ? match[5] + (match[6] || 1) : 2 * ( match[3] === "even" || match[3] === "odd" ) );
				match[5] = +( ( match[7] + match[8] ) || match[3] === "odd" );

			// other types prohibit arguments
			} else if ( match[3] ) {
				Sizzle.error( match[0] );
			}

			return match;
		},

		"PSEUDO": function( match ) {
			var excess,
				unquoted = !match[6] && match[2];

			if ( matchExpr["CHILD"].test( match[0] ) ) {
				return null;
			}

			// Accept quoted arguments as-is
			if ( match[3] ) {
				match[2] = match[4] || match[5] || "";

			// Strip excess characters from unquoted arguments
			} else if ( unquoted && rpseudo.test( unquoted ) &&
				// Get excess from tokenize (recursively)
				(excess = tokenize( unquoted, true )) &&
				// advance to the next closing parenthesis
				(excess = unquoted.indexOf( ")", unquoted.length - excess ) - unquoted.length) ) {

				// excess is a negative index
				match[0] = match[0].slice( 0, excess );
				match[2] = unquoted.slice( 0, excess );
			}

			// Return only captures needed by the pseudo filter method (type and argument)
			return match.slice( 0, 3 );
		}
	},

	filter: {

		"TAG": function( nodeNameSelector ) {
			var nodeName = nodeNameSelector.replace( runescape, funescape ).toLowerCase();
			return nodeNameSelector === "*" ?
				function() { return true; } :
				function( elem ) {
					return elem.nodeName && elem.nodeName.toLowerCase() === nodeName;
				};
		},

		"CLASS": function( className ) {
			var pattern = classCache[ className + " " ];

			return pattern ||
				(pattern = new RegExp( "(^|" + whitespace + ")" + className + "(" + whitespace + "|$)" )) &&
				classCache( className, function( elem ) {
					return pattern.test( typeof elem.className === "string" && elem.className || typeof elem.getAttribute !== "undefined" && elem.getAttribute("class") || "" );
				});
		},

		"ATTR": function( name, operator, check ) {
			return function( elem ) {
				var result = Sizzle.attr( elem, name );

				if ( result == null ) {
					return operator === "!=";
				}
				if ( !operator ) {
					return true;
				}

				result += "";

				return operator === "=" ? result === check :
					operator === "!=" ? result !== check :
					operator === "^=" ? check && result.indexOf( check ) === 0 :
					operator === "*=" ? check && result.indexOf( check ) > -1 :
					operator === "$=" ? check && result.slice( -check.length ) === check :
					operator === "~=" ? ( " " + result.replace( rwhitespace, " " ) + " " ).indexOf( check ) > -1 :
					operator === "|=" ? result === check || result.slice( 0, check.length + 1 ) === check + "-" :
					false;
			};
		},

		"CHILD": function( type, what, argument, first, last ) {
			var simple = type.slice( 0, 3 ) !== "nth",
				forward = type.slice( -4 ) !== "last",
				ofType = what === "of-type";

			return first === 1 && last === 0 ?

				// Shortcut for :nth-*(n)
				function( elem ) {
					return !!elem.parentNode;
				} :

				function( elem, context, xml ) {
					var cache, outerCache, node, diff, nodeIndex, start,
						dir = simple !== forward ? "nextSibling" : "previousSibling",
						parent = elem.parentNode,
						name = ofType && elem.nodeName.toLowerCase(),
						useCache = !xml && !ofType;

					if ( parent ) {

						// :(first|last|only)-(child|of-type)
						if ( simple ) {
							while ( dir ) {
								node = elem;
								while ( (node = node[ dir ]) ) {
									if ( ofType ? node.nodeName.toLowerCase() === name : node.nodeType === 1 ) {
										return false;
									}
								}
								// Reverse direction for :only-* (if we haven't yet done so)
								start = dir = type === "only" && !start && "nextSibling";
							}
							return true;
						}

						start = [ forward ? parent.firstChild : parent.lastChild ];

						// non-xml :nth-child(...) stores cache data on `parent`
						if ( forward && useCache ) {
							// Seek `elem` from a previously-cached index
							outerCache = parent[ expando ] || (parent[ expando ] = {});
							cache = outerCache[ type ] || [];
							nodeIndex = cache[0] === dirruns && cache[1];
							diff = cache[0] === dirruns && cache[2];
							node = nodeIndex && parent.childNodes[ nodeIndex ];

							while ( (node = ++nodeIndex && node && node[ dir ] ||

								// Fallback to seeking `elem` from the start
								(diff = nodeIndex = 0) || start.pop()) ) {

								// When found, cache indexes on `parent` and break
								if ( node.nodeType === 1 && ++diff && node === elem ) {
									outerCache[ type ] = [ dirruns, nodeIndex, diff ];
									break;
								}
							}

						// Use previously-cached element index if available
						} else if ( useCache && (cache = (elem[ expando ] || (elem[ expando ] = {}))[ type ]) && cache[0] === dirruns ) {
							diff = cache[1];

						// xml :nth-child(...) or :nth-last-child(...) or :nth(-last)?-of-type(...)
						} else {
							// Use the same loop as above to seek `elem` from the start
							while ( (node = ++nodeIndex && node && node[ dir ] ||
								(diff = nodeIndex = 0) || start.pop()) ) {

								if ( ( ofType ? node.nodeName.toLowerCase() === name : node.nodeType === 1 ) && ++diff ) {
									// Cache the index of each encountered element
									if ( useCache ) {
										(node[ expando ] || (node[ expando ] = {}))[ type ] = [ dirruns, diff ];
									}

									if ( node === elem ) {
										break;
									}
								}
							}
						}

						// Incorporate the offset, then check against cycle size
						diff -= last;
						return diff === first || ( diff % first === 0 && diff / first >= 0 );
					}
				};
		},

		"PSEUDO": function( pseudo, argument ) {
			// pseudo-class names are case-insensitive
			// http://www.w3.org/TR/selectors/#pseudo-classes
			// Prioritize by case sensitivity in case custom pseudos are added with uppercase letters
			// Remember that setFilters inherits from pseudos
			var args,
				fn = Expr.pseudos[ pseudo ] || Expr.setFilters[ pseudo.toLowerCase() ] ||
					Sizzle.error( "unsupported pseudo: " + pseudo );

			// The user may use createPseudo to indicate that
			// arguments are needed to create the filter function
			// just as Sizzle does
			if ( fn[ expando ] ) {
				return fn( argument );
			}

			// But maintain support for old signatures
			if ( fn.length > 1 ) {
				args = [ pseudo, pseudo, "", argument ];
				return Expr.setFilters.hasOwnProperty( pseudo.toLowerCase() ) ?
					markFunction(function( seed, matches ) {
						var idx,
							matched = fn( seed, argument ),
							i = matched.length;
						while ( i-- ) {
							idx = indexOf( seed, matched[i] );
							seed[ idx ] = !( matches[ idx ] = matched[i] );
						}
					}) :
					function( elem ) {
						return fn( elem, 0, args );
					};
			}

			return fn;
		}
	},

	pseudos: {
		// Potentially complex pseudos
		"not": markFunction(function( selector ) {
			// Trim the selector passed to compile
			// to avoid treating leading and trailing
			// spaces as combinators
			var input = [],
				results = [],
				matcher = compile( selector.replace( rtrim, "$1" ) );

			return matcher[ expando ] ?
				markFunction(function( seed, matches, context, xml ) {
					var elem,
						unmatched = matcher( seed, null, xml, [] ),
						i = seed.length;

					// Match elements unmatched by `matcher`
					while ( i-- ) {
						if ( (elem = unmatched[i]) ) {
							seed[i] = !(matches[i] = elem);
						}
					}
				}) :
				function( elem, context, xml ) {
					input[0] = elem;
					matcher( input, null, xml, results );
					// Don't keep the element (issue #299)
					input[0] = null;
					return !results.pop();
				};
		}),

		"has": markFunction(function( selector ) {
			return function( elem ) {
				return Sizzle( selector, elem ).length > 0;
			};
		}),

		"contains": markFunction(function( text ) {
			text = text.replace( runescape, funescape );
			return function( elem ) {
				return ( elem.textContent || elem.innerText || getText( elem ) ).indexOf( text ) > -1;
			};
		}),

		// "Whether an element is represented by a :lang() selector
		// is based solely on the element's language value
		// being equal to the identifier C,
		// or beginning with the identifier C immediately followed by "-".
		// The matching of C against the element's language value is performed case-insensitively.
		// The identifier C does not have to be a valid language name."
		// http://www.w3.org/TR/selectors/#lang-pseudo
		"lang": markFunction( function( lang ) {
			// lang value must be a valid identifier
			if ( !ridentifier.test(lang || "") ) {
				Sizzle.error( "unsupported lang: " + lang );
			}
			lang = lang.replace( runescape, funescape ).toLowerCase();
			return function( elem ) {
				var elemLang;
				do {
					if ( (elemLang = documentIsHTML ?
						elem.lang :
						elem.getAttribute("xml:lang") || elem.getAttribute("lang")) ) {

						elemLang = elemLang.toLowerCase();
						return elemLang === lang || elemLang.indexOf( lang + "-" ) === 0;
					}
				} while ( (elem = elem.parentNode) && elem.nodeType === 1 );
				return false;
			};
		}),

		// Miscellaneous
		"target": function( elem ) {
			var hash = window.location && window.location.hash;
			return hash && hash.slice( 1 ) === elem.id;
		},

		"root": function( elem ) {
			return elem === docElem;
		},

		"focus": function( elem ) {
			return elem === document.activeElement && (!document.hasFocus || document.hasFocus()) && !!(elem.type || elem.href || ~elem.tabIndex);
		},

		// Boolean properties
		"enabled": function( elem ) {
			return elem.disabled === false;
		},

		"disabled": function( elem ) {
			return elem.disabled === true;
		},

		"checked": function( elem ) {
			// In CSS3, :checked should return both checked and selected elements
			// http://www.w3.org/TR/2011/REC-css3-selectors-20110929/#checked
			var nodeName = elem.nodeName.toLowerCase();
			return (nodeName === "input" && !!elem.checked) || (nodeName === "option" && !!elem.selected);
		},

		"selected": function( elem ) {
			// Accessing this property makes selected-by-default
			// options in Safari work properly
			if ( elem.parentNode ) {
				elem.parentNode.selectedIndex;
			}

			return elem.selected === true;
		},

		// Contents
		"empty": function( elem ) {
			// http://www.w3.org/TR/selectors/#empty-pseudo
			// :empty is negated by element (1) or content nodes (text: 3; cdata: 4; entity ref: 5),
			//   but not by others (comment: 8; processing instruction: 7; etc.)
			// nodeType < 6 works because attributes (2) do not appear as children
			for ( elem = elem.firstChild; elem; elem = elem.nextSibling ) {
				if ( elem.nodeType < 6 ) {
					return false;
				}
			}
			return true;
		},

		"parent": function( elem ) {
			return !Expr.pseudos["empty"]( elem );
		},

		// Element/input types
		"header": function( elem ) {
			return rheader.test( elem.nodeName );
		},

		"input": function( elem ) {
			return rinputs.test( elem.nodeName );
		},

		"button": function( elem ) {
			var name = elem.nodeName.toLowerCase();
			return name === "input" && elem.type === "button" || name === "button";
		},

		"text": function( elem ) {
			var attr;
			return elem.nodeName.toLowerCase() === "input" &&
				elem.type === "text" &&

				// Support: IE<8
				// New HTML5 attribute values (e.g., "search") appear with elem.type === "text"
				( (attr = elem.getAttribute("type")) == null || attr.toLowerCase() === "text" );
		},

		// Position-in-collection
		"first": createPositionalPseudo(function() {
			return [ 0 ];
		}),

		"last": createPositionalPseudo(function( matchIndexes, length ) {
			return [ length - 1 ];
		}),

		"eq": createPositionalPseudo(function( matchIndexes, length, argument ) {
			return [ argument < 0 ? argument + length : argument ];
		}),

		"even": createPositionalPseudo(function( matchIndexes, length ) {
			var i = 0;
			for ( ; i < length; i += 2 ) {
				matchIndexes.push( i );
			}
			return matchIndexes;
		}),

		"odd": createPositionalPseudo(function( matchIndexes, length ) {
			var i = 1;
			for ( ; i < length; i += 2 ) {
				matchIndexes.push( i );
			}
			return matchIndexes;
		}),

		"lt": createPositionalPseudo(function( matchIndexes, length, argument ) {
			var i = argument < 0 ? argument + length : argument;
			for ( ; --i >= 0; ) {
				matchIndexes.push( i );
			}
			return matchIndexes;
		}),

		"gt": createPositionalPseudo(function( matchIndexes, length, argument ) {
			var i = argument < 0 ? argument + length : argument;
			for ( ; ++i < length; ) {
				matchIndexes.push( i );
			}
			return matchIndexes;
		})
	}
};

Expr.pseudos["nth"] = Expr.pseudos["eq"];

// Add button/input type pseudos
for ( i in { radio: true, checkbox: true, file: true, password: true, image: true } ) {
	Expr.pseudos[ i ] = createInputPseudo( i );
}
for ( i in { submit: true, reset: true } ) {
	Expr.pseudos[ i ] = createButtonPseudo( i );
}

// Easy API for creating new setFilters
function setFilters() {}
setFilters.prototype = Expr.filters = Expr.pseudos;
Expr.setFilters = new setFilters();

tokenize = Sizzle.tokenize = function( selector, parseOnly ) {
	var matched, match, tokens, type,
		soFar, groups, preFilters,
		cached = tokenCache[ selector + " " ];

	if ( cached ) {
		return parseOnly ? 0 : cached.slice( 0 );
	}

	soFar = selector;
	groups = [];
	preFilters = Expr.preFilter;

	while ( soFar ) {

		// Comma and first run
		if ( !matched || (match = rcomma.exec( soFar )) ) {
			if ( match ) {
				// Don't consume trailing commas as valid
				soFar = soFar.slice( match[0].length ) || soFar;
			}
			groups.push( (tokens = []) );
		}

		matched = false;

		// Combinators
		if ( (match = rcombinators.exec( soFar )) ) {
			matched = match.shift();
			tokens.push({
				value: matched,
				// Cast descendant combinators to space
				type: match[0].replace( rtrim, " " )
			});
			soFar = soFar.slice( matched.length );
		}

		// Filters
		for ( type in Expr.filter ) {
			if ( (match = matchExpr[ type ].exec( soFar )) && (!preFilters[ type ] ||
				(match = preFilters[ type ]( match ))) ) {
				matched = match.shift();
				tokens.push({
					value: matched,
					type: type,
					matches: match
				});
				soFar = soFar.slice( matched.length );
			}
		}

		if ( !matched ) {
			break;
		}
	}

	// Return the length of the invalid excess
	// if we're just parsing
	// Otherwise, throw an error or return tokens
	return parseOnly ?
		soFar.length :
		soFar ?
			Sizzle.error( selector ) :
			// Cache the tokens
			tokenCache( selector, groups ).slice( 0 );
};

function toSelector( tokens ) {
	var i = 0,
		len = tokens.length,
		selector = "";
	for ( ; i < len; i++ ) {
		selector += tokens[i].value;
	}
	return selector;
}

function addCombinator( matcher, combinator, base ) {
	var dir = combinator.dir,
		checkNonElements = base && dir === "parentNode",
		doneName = done++;

	return combinator.first ?
		// Check against closest ancestor/preceding element
		function( elem, context, xml ) {
			while ( (elem = elem[ dir ]) ) {
				if ( elem.nodeType === 1 || checkNonElements ) {
					return matcher( elem, context, xml );
				}
			}
		} :

		// Check against all ancestor/preceding elements
		function( elem, context, xml ) {
			var oldCache, outerCache,
				newCache = [ dirruns, doneName ];

			// We can't set arbitrary data on XML nodes, so they don't benefit from dir caching
			if ( xml ) {
				while ( (elem = elem[ dir ]) ) {
					if ( elem.nodeType === 1 || checkNonElements ) {
						if ( matcher( elem, context, xml ) ) {
							return true;
						}
					}
				}
			} else {
				while ( (elem = elem[ dir ]) ) {
					if ( elem.nodeType === 1 || checkNonElements ) {
						outerCache = elem[ expando ] || (elem[ expando ] = {});
						if ( (oldCache = outerCache[ dir ]) &&
							oldCache[ 0 ] === dirruns && oldCache[ 1 ] === doneName ) {

							// Assign to newCache so results back-propagate to previous elements
							return (newCache[ 2 ] = oldCache[ 2 ]);
						} else {
							// Reuse newcache so results back-propagate to previous elements
							outerCache[ dir ] = newCache;

							// A match means we're done; a fail means we have to keep checking
							if ( (newCache[ 2 ] = matcher( elem, context, xml )) ) {
								return true;
							}
						}
					}
				}
			}
		};
}

function elementMatcher( matchers ) {
	return matchers.length > 1 ?
		function( elem, context, xml ) {
			var i = matchers.length;
			while ( i-- ) {
				if ( !matchers[i]( elem, context, xml ) ) {
					return false;
				}
			}
			return true;
		} :
		matchers[0];
}

function multipleContexts( selector, contexts, results ) {
	var i = 0,
		len = contexts.length;
	for ( ; i < len; i++ ) {
		Sizzle( selector, contexts[i], results );
	}
	return results;
}

function condense( unmatched, map, filter, context, xml ) {
	var elem,
		newUnmatched = [],
		i = 0,
		len = unmatched.length,
		mapped = map != null;

	for ( ; i < len; i++ ) {
		if ( (elem = unmatched[i]) ) {
			if ( !filter || filter( elem, context, xml ) ) {
				newUnmatched.push( elem );
				if ( mapped ) {
					map.push( i );
				}
			}
		}
	}

	return newUnmatched;
}

function setMatcher( preFilter, selector, matcher, postFilter, postFinder, postSelector ) {
	if ( postFilter && !postFilter[ expando ] ) {
		postFilter = setMatcher( postFilter );
	}
	if ( postFinder && !postFinder[ expando ] ) {
		postFinder = setMatcher( postFinder, postSelector );
	}
	return markFunction(function( seed, results, context, xml ) {
		var temp, i, elem,
			preMap = [],
			postMap = [],
			preexisting = results.length,

			// Get initial elements from seed or context
			elems = seed || multipleContexts( selector || "*", context.nodeType ? [ context ] : context, [] ),

			// Prefilter to get matcher input, preserving a map for seed-results synchronization
			matcherIn = preFilter && ( seed || !selector ) ?
				condense( elems, preMap, preFilter, context, xml ) :
				elems,

			matcherOut = matcher ?
				// If we have a postFinder, or filtered seed, or non-seed postFilter or preexisting results,
				postFinder || ( seed ? preFilter : preexisting || postFilter ) ?

					// ...intermediate processing is necessary
					[] :

					// ...otherwise use results directly
					results :
				matcherIn;

		// Find primary matches
		if ( matcher ) {
			matcher( matcherIn, matcherOut, context, xml );
		}

		// Apply postFilter
		if ( postFilter ) {
			temp = condense( matcherOut, postMap );
			postFilter( temp, [], context, xml );

			// Un-match failing elements by moving them back to matcherIn
			i = temp.length;
			while ( i-- ) {
				if ( (elem = temp[i]) ) {
					matcherOut[ postMap[i] ] = !(matcherIn[ postMap[i] ] = elem);
				}
			}
		}

		if ( seed ) {
			if ( postFinder || preFilter ) {
				if ( postFinder ) {
					// Get the final matcherOut by condensing this intermediate into postFinder contexts
					temp = [];
					i = matcherOut.length;
					while ( i-- ) {
						if ( (elem = matcherOut[i]) ) {
							// Restore matcherIn since elem is not yet a final match
							temp.push( (matcherIn[i] = elem) );
						}
					}
					postFinder( null, (matcherOut = []), temp, xml );
				}

				// Move matched elements from seed to results to keep them synchronized
				i = matcherOut.length;
				while ( i-- ) {
					if ( (elem = matcherOut[i]) &&
						(temp = postFinder ? indexOf( seed, elem ) : preMap[i]) > -1 ) {

						seed[temp] = !(results[temp] = elem);
					}
				}
			}

		// Add elements to results, through postFinder if defined
		} else {
			matcherOut = condense(
				matcherOut === results ?
					matcherOut.splice( preexisting, matcherOut.length ) :
					matcherOut
			);
			if ( postFinder ) {
				postFinder( null, results, matcherOut, xml );
			} else {
				push.apply( results, matcherOut );
			}
		}
	});
}

function matcherFromTokens( tokens ) {
	var checkContext, matcher, j,
		len = tokens.length,
		leadingRelative = Expr.relative[ tokens[0].type ],
		implicitRelative = leadingRelative || Expr.relative[" "],
		i = leadingRelative ? 1 : 0,

		// The foundational matcher ensures that elements are reachable from top-level context(s)
		matchContext = addCombinator( function( elem ) {
			return elem === checkContext;
		}, implicitRelative, true ),
		matchAnyContext = addCombinator( function( elem ) {
			return indexOf( checkContext, elem ) > -1;
		}, implicitRelative, true ),
		matchers = [ function( elem, context, xml ) {
			var ret = ( !leadingRelative && ( xml || context !== outermostContext ) ) || (
				(checkContext = context).nodeType ?
					matchContext( elem, context, xml ) :
					matchAnyContext( elem, context, xml ) );
			// Avoid hanging onto element (issue #299)
			checkContext = null;
			return ret;
		} ];

	for ( ; i < len; i++ ) {
		if ( (matcher = Expr.relative[ tokens[i].type ]) ) {
			matchers = [ addCombinator(elementMatcher( matchers ), matcher) ];
		} else {
			matcher = Expr.filter[ tokens[i].type ].apply( null, tokens[i].matches );

			// Return special upon seeing a positional matcher
			if ( matcher[ expando ] ) {
				// Find the next relative operator (if any) for proper handling
				j = ++i;
				for ( ; j < len; j++ ) {
					if ( Expr.relative[ tokens[j].type ] ) {
						break;
					}
				}
				return setMatcher(
					i > 1 && elementMatcher( matchers ),
					i > 1 && toSelector(
						// If the preceding token was a descendant combinator, insert an implicit any-element `*`
						tokens.slice( 0, i - 1 ).concat({ value: tokens[ i - 2 ].type === " " ? "*" : "" })
					).replace( rtrim, "$1" ),
					matcher,
					i < j && matcherFromTokens( tokens.slice( i, j ) ),
					j < len && matcherFromTokens( (tokens = tokens.slice( j )) ),
					j < len && toSelector( tokens )
				);
			}
			matchers.push( matcher );
		}
	}

	return elementMatcher( matchers );
}

function matcherFromGroupMatchers( elementMatchers, setMatchers ) {
	var bySet = setMatchers.length > 0,
		byElement = elementMatchers.length > 0,
		superMatcher = function( seed, context, xml, results, outermost ) {
			var elem, j, matcher,
				matchedCount = 0,
				i = "0",
				unmatched = seed && [],
				setMatched = [],
				contextBackup = outermostContext,
				// We must always have either seed elements or outermost context
				elems = seed || byElement && Expr.find["TAG"]( "*", outermost ),
				// Use integer dirruns iff this is the outermost matcher
				dirrunsUnique = (dirruns += contextBackup == null ? 1 : Math.random() || 0.1),
				len = elems.length;

			if ( outermost ) {
				outermostContext = context !== document && context;
			}

			// Add elements passing elementMatchers directly to results
			// Keep `i` a string if there are no elements so `matchedCount` will be "00" below
			// Support: IE<9, Safari
			// Tolerate NodeList properties (IE: "length"; Safari: <number>) matching elements by id
			for ( ; i !== len && (elem = elems[i]) != null; i++ ) {
				if ( byElement && elem ) {
					j = 0;
					while ( (matcher = elementMatchers[j++]) ) {
						if ( matcher( elem, context, xml ) ) {
							results.push( elem );
							break;
						}
					}
					if ( outermost ) {
						dirruns = dirrunsUnique;
					}
				}

				// Track unmatched elements for set filters
				if ( bySet ) {
					// They will have gone through all possible matchers
					if ( (elem = !matcher && elem) ) {
						matchedCount--;
					}

					// Lengthen the array for every element, matched or not
					if ( seed ) {
						unmatched.push( elem );
					}
				}
			}

			// Apply set filters to unmatched elements
			matchedCount += i;
			if ( bySet && i !== matchedCount ) {
				j = 0;
				while ( (matcher = setMatchers[j++]) ) {
					matcher( unmatched, setMatched, context, xml );
				}

				if ( seed ) {
					// Reintegrate element matches to eliminate the need for sorting
					if ( matchedCount > 0 ) {
						while ( i-- ) {
							if ( !(unmatched[i] || setMatched[i]) ) {
								setMatched[i] = pop.call( results );
							}
						}
					}

					// Discard index placeholder values to get only actual matches
					setMatched = condense( setMatched );
				}

				// Add matches to results
				push.apply( results, setMatched );

				// Seedless set matches succeeding multiple successful matchers stipulate sorting
				if ( outermost && !seed && setMatched.length > 0 &&
					( matchedCount + setMatchers.length ) > 1 ) {

					Sizzle.uniqueSort( results );
				}
			}

			// Override manipulation of globals by nested matchers
			if ( outermost ) {
				dirruns = dirrunsUnique;
				outermostContext = contextBackup;
			}

			return unmatched;
		};

	return bySet ?
		markFunction( superMatcher ) :
		superMatcher;
}

compile = Sizzle.compile = function( selector, match /* Internal Use Only */ ) {
	var i,
		setMatchers = [],
		elementMatchers = [],
		cached = compilerCache[ selector + " " ];

	if ( !cached ) {
		// Generate a function of recursive functions that can be used to check each element
		if ( !match ) {
			match = tokenize( selector );
		}
		i = match.length;
		while ( i-- ) {
			cached = matcherFromTokens( match[i] );
			if ( cached[ expando ] ) {
				setMatchers.push( cached );
			} else {
				elementMatchers.push( cached );
			}
		}

		// Cache the compiled function
		cached = compilerCache( selector, matcherFromGroupMatchers( elementMatchers, setMatchers ) );

		// Save selector and tokenization
		cached.selector = selector;
	}
	return cached;
};

/**
 * A low-level selection function that works with Sizzle's compiled
 *  selector functions
 * @param {String|Function} selector A selector or a pre-compiled
 *  selector function built with Sizzle.compile
 * @param {Element} context
 * @param {Array} [results]
 * @param {Array} [seed] A set of elements to match against
 */
select = Sizzle.select = function( selector, context, results, seed ) {
	var i, tokens, token, type, find,
		compiled = typeof selector === "function" && selector,
		match = !seed && tokenize( (selector = compiled.selector || selector) );

	results = results || [];

	// Try to minimize operations if there is no seed and only one group
	if ( match.length === 1 ) {

		// Take a shortcut and set the context if the root selector is an ID
		tokens = match[0] = match[0].slice( 0 );
		if ( tokens.length > 2 && (token = tokens[0]).type === "ID" &&
				support.getById && context.nodeType === 9 && documentIsHTML &&
				Expr.relative[ tokens[1].type ] ) {

			context = ( Expr.find["ID"]( token.matches[0].replace(runescape, funescape), context ) || [] )[0];
			if ( !context ) {
				return results;

			// Precompiled matchers will still verify ancestry, so step up a level
			} else if ( compiled ) {
				context = context.parentNode;
			}

			selector = selector.slice( tokens.shift().value.length );
		}

		// Fetch a seed set for right-to-left matching
		i = matchExpr["needsContext"].test( selector ) ? 0 : tokens.length;
		while ( i-- ) {
			token = tokens[i];

			// Abort if we hit a combinator
			if ( Expr.relative[ (type = token.type) ] ) {
				break;
			}
			if ( (find = Expr.find[ type ]) ) {
				// Search, expanding context for leading sibling combinators
				if ( (seed = find(
					token.matches[0].replace( runescape, funescape ),
					rsibling.test( tokens[0].type ) && testContext( context.parentNode ) || context
				)) ) {

					// If seed is empty or no tokens remain, we can return early
					tokens.splice( i, 1 );
					selector = seed.length && toSelector( tokens );
					if ( !selector ) {
						push.apply( results, seed );
						return results;
					}

					break;
				}
			}
		}
	}

	// Compile and execute a filtering function if one is not provided
	// Provide `match` to avoid retokenization if we modified the selector above
	( compiled || compile( selector, match ) )(
		seed,
		context,
		!documentIsHTML,
		results,
		rsibling.test( selector ) && testContext( context.parentNode ) || context
	);
	return results;
};

// One-time assignments

// Sort stability
support.sortStable = expando.split("").sort( sortOrder ).join("") === expando;

// Support: Chrome 14-35+
// Always assume duplicates if they aren't passed to the comparison function
support.detectDuplicates = !!hasDuplicate;

// Initialize against the default document
setDocument();

// Support: Webkit<537.32 - Safari 6.0.3/Chrome 25 (fixed in Chrome 27)
// Detached nodes confoundingly follow *each other*
support.sortDetached = assert(function( div1 ) {
	// Should return 1, but returns 4 (following)
	return div1.compareDocumentPosition( document.createElement("div") ) & 1;
});

// Support: IE<8
// Prevent attribute/property "interpolation"
// http://msdn.microsoft.com/en-us/library/ms536429%28VS.85%29.aspx
if ( !assert(function( div ) {
	div.innerHTML = "<a href='#'></a>";
	return div.firstChild.getAttribute("href") === "#" ;
}) ) {
	addHandle( "type|href|height|width", function( elem, name, isXML ) {
		if ( !isXML ) {
			return elem.getAttribute( name, name.toLowerCase() === "type" ? 1 : 2 );
		}
	});
}

// Support: IE<9
// Use defaultValue in place of getAttribute("value")
if ( !support.attributes || !assert(function( div ) {
	div.innerHTML = "<input/>";
	div.firstChild.setAttribute( "value", "" );
	return div.firstChild.getAttribute( "value" ) === "";
}) ) {
	addHandle( "value", function( elem, name, isXML ) {
		if ( !isXML && elem.nodeName.toLowerCase() === "input" ) {
			return elem.defaultValue;
		}
	});
}

// Support: IE<9
// Use getAttributeNode to fetch booleans when getAttribute lies
if ( !assert(function( div ) {
	return div.getAttribute("disabled") == null;
}) ) {
	addHandle( booleans, function( elem, name, isXML ) {
		var val;
		if ( !isXML ) {
			return elem[ name ] === true ? name.toLowerCase() :
					(val = elem.getAttributeNode( name )) && val.specified ?
					val.value :
				null;
		}
	});
}

return Sizzle;

})( window );



jQuery.find = Sizzle;
jQuery.expr = Sizzle.selectors;
jQuery.expr[":"] = jQuery.expr.pseudos;
jQuery.unique = Sizzle.uniqueSort;
jQuery.text = Sizzle.getText;
jQuery.isXMLDoc = Sizzle.isXML;
jQuery.contains = Sizzle.contains;



var rneedsContext = jQuery.expr.match.needsContext;

var rsingleTag = (/^<(\w+)\s*\/?>(?:<\/\1>|)$/);



var risSimple = /^.[^:#\[\.,]*$/;

// Implement the identical functionality for filter and not
function winnow( elements, qualifier, not ) {
	if ( jQuery.isFunction( qualifier ) ) {
		return jQuery.grep( elements, function( elem, i ) {
			/* jshint -W018 */
			return !!qualifier.call( elem, i, elem ) !== not;
		});

	}

	if ( qualifier.nodeType ) {
		return jQuery.grep( elements, function( elem ) {
			return ( elem === qualifier ) !== not;
		});

	}

	if ( typeof qualifier === "string" ) {
		if ( risSimple.test( qualifier ) ) {
			return jQuery.filter( qualifier, elements, not );
		}

		qualifier = jQuery.filter( qualifier, elements );
	}

	return jQuery.grep( elements, function( elem ) {
		return ( indexOf.call( qualifier, elem ) >= 0 ) !== not;
	});
}

jQuery.filter = function( expr, elems, not ) {
	var elem = elems[ 0 ];

	if ( not ) {
		expr = ":not(" + expr + ")";
	}

	return elems.length === 1 && elem.nodeType === 1 ?
		jQuery.find.matchesSelector( elem, expr ) ? [ elem ] : [] :
		jQuery.find.matches( expr, jQuery.grep( elems, function( elem ) {
			return elem.nodeType === 1;
		}));
};

jQuery.fn.extend({
	find: function( selector ) {
		var i,
			len = this.length,
			ret = [],
			self = this;

		if ( typeof selector !== "string" ) {
			return this.pushStack( jQuery( selector ).filter(function() {
				for ( i = 0; i < len; i++ ) {
					if ( jQuery.contains( self[ i ], this ) ) {
						return true;
					}
				}
			}) );
		}

		for ( i = 0; i < len; i++ ) {
			jQuery.find( selector, self[ i ], ret );
		}

		// Needed because $( selector, context ) becomes $( context ).find( selector )
		ret = this.pushStack( len > 1 ? jQuery.unique( ret ) : ret );
		ret.selector = this.selector ? this.selector + " " + selector : selector;
		return ret;
	},
	filter: function( selector ) {
		return this.pushStack( winnow(this, selector || [], false) );
	},
	not: function( selector ) {
		return this.pushStack( winnow(this, selector || [], true) );
	},
	is: function( selector ) {
		return !!winnow(
			this,

			// If this is a positional/relative selector, check membership in the returned set
			// so $("p:first").is("p:last") won't return true for a doc with two "p".
			typeof selector === "string" && rneedsContext.test( selector ) ?
				jQuery( selector ) :
				selector || [],
			false
		).length;
	}
});


// Initialize a jQuery object


// A central reference to the root jQuery(document)
var rootjQuery,

	// A simple way to check for HTML strings
	// Prioritize #id over <tag> to avoid XSS via location.hash (#9521)
	// Strict HTML recognition (#11290: must start with <)
	rquickExpr = /^(?:\s*(<[\w\W]+>)[^>]*|#([\w-]*))$/,

	init = jQuery.fn.init = function( selector, context ) {
		var match, elem;

		// HANDLE: $(""), $(null), $(undefined), $(false)
		if ( !selector ) {
			return this;
		}

		// Handle HTML strings
		if ( typeof selector === "string" ) {
			if ( selector[0] === "<" && selector[ selector.length - 1 ] === ">" && selector.length >= 3 ) {
				// Assume that strings that start and end with <> are HTML and skip the regex check
				match = [ null, selector, null ];

			} else {
				match = rquickExpr.exec( selector );
			}

			// Match html or make sure no context is specified for #id
			if ( match && (match[1] || !context) ) {

				// HANDLE: $(html) -> $(array)
				if ( match[1] ) {
					context = context instanceof jQuery ? context[0] : context;

					// Option to run scripts is true for back-compat
					// Intentionally let the error be thrown if parseHTML is not present
					jQuery.merge( this, jQuery.parseHTML(
						match[1],
						context && context.nodeType ? context.ownerDocument || context : document,
						true
					) );

					// HANDLE: $(html, props)
					if ( rsingleTag.test( match[1] ) && jQuery.isPlainObject( context ) ) {
						for ( match in context ) {
							// Properties of context are called as methods if possible
							if ( jQuery.isFunction( this[ match ] ) ) {
								this[ match ]( context[ match ] );

							// ...and otherwise set as attributes
							} else {
								this.attr( match, context[ match ] );
							}
						}
					}

					return this;

				// HANDLE: $(#id)
				} else {
					elem = document.getElementById( match[2] );

					// Support: Blackberry 4.6
					// gEBID returns nodes no longer in the document (#6963)
					if ( elem && elem.parentNode ) {
						// Inject the element directly into the jQuery object
						this.length = 1;
						this[0] = elem;
					}

					this.context = document;
					this.selector = selector;
					return this;
				}

			// HANDLE: $(expr, $(...))
			} else if ( !context || context.jquery ) {
				return ( context || rootjQuery ).find( selector );

			// HANDLE: $(expr, context)
			// (which is just equivalent to: $(context).find(expr)
			} else {
				return this.constructor( context ).find( selector );
			}

		// HANDLE: $(DOMElement)
		} else if ( selector.nodeType ) {
			this.context = this[0] = selector;
			this.length = 1;
			return this;

		// HANDLE: $(function)
		// Shortcut for document ready
		} else if ( jQuery.isFunction( selector ) ) {
			return typeof rootjQuery.ready !== "undefined" ?
				rootjQuery.ready( selector ) :
				// Execute immediately if ready is not present
				selector( jQuery );
		}

		if ( selector.selector !== undefined ) {
			this.selector = selector.selector;
			this.context = selector.context;
		}

		return jQuery.makeArray( selector, this );
	};

// Give the init function the jQuery prototype for later instantiation
init.prototype = jQuery.fn;

// Initialize central reference
rootjQuery = jQuery( document );


var rparentsprev = /^(?:parents|prev(?:Until|All))/,
	// Methods guaranteed to produce a unique set when starting from a unique set
	guaranteedUnique = {
		children: true,
		contents: true,
		next: true,
		prev: true
	};

jQuery.extend({
	dir: function( elem, dir, until ) {
		var matched = [],
			truncate = until !== undefined;

		while ( (elem = elem[ dir ]) && elem.nodeType !== 9 ) {
			if ( elem.nodeType === 1 ) {
				if ( truncate && jQuery( elem ).is( until ) ) {
					break;
				}
				matched.push( elem );
			}
		}
		return matched;
	},

	sibling: function( n, elem ) {
		var matched = [];

		for ( ; n; n = n.nextSibling ) {
			if ( n.nodeType === 1 && n !== elem ) {
				matched.push( n );
			}
		}

		return matched;
	}
});

jQuery.fn.extend({
	has: function( target ) {
		var targets = jQuery( target, this ),
			l = targets.length;

		return this.filter(function() {
			var i = 0;
			for ( ; i < l; i++ ) {
				if ( jQuery.contains( this, targets[i] ) ) {
					return true;
				}
			}
		});
	},

	closest: function( selectors, context ) {
		var cur,
			i = 0,
			l = this.length,
			matched = [],
			pos = rneedsContext.test( selectors ) || typeof selectors !== "string" ?
				jQuery( selectors, context || this.context ) :
				0;

		for ( ; i < l; i++ ) {
			for ( cur = this[i]; cur && cur !== context; cur = cur.parentNode ) {
				// Always skip document fragments
				if ( cur.nodeType < 11 && (pos ?
					pos.index(cur) > -1 :

					// Don't pass non-elements to Sizzle
					cur.nodeType === 1 &&
						jQuery.find.matchesSelector(cur, selectors)) ) {

					matched.push( cur );
					break;
				}
			}
		}

		return this.pushStack( matched.length > 1 ? jQuery.unique( matched ) : matched );
	},

	// Determine the position of an element within the set
	index: function( elem ) {

		// No argument, return index in parent
		if ( !elem ) {
			return ( this[ 0 ] && this[ 0 ].parentNode ) ? this.first().prevAll().length : -1;
		}

		// Index in selector
		if ( typeof elem === "string" ) {
			return indexOf.call( jQuery( elem ), this[ 0 ] );
		}

		// Locate the position of the desired element
		return indexOf.call( this,

			// If it receives a jQuery object, the first element is used
			elem.jquery ? elem[ 0 ] : elem
		);
	},

	add: function( selector, context ) {
		return this.pushStack(
			jQuery.unique(
				jQuery.merge( this.get(), jQuery( selector, context ) )
			)
		);
	},

	addBack: function( selector ) {
		return this.add( selector == null ?
			this.prevObject : this.prevObject.filter(selector)
		);
	}
});

function sibling( cur, dir ) {
	while ( (cur = cur[dir]) && cur.nodeType !== 1 ) {}
	return cur;
}

jQuery.each({
	parent: function( elem ) {
		var parent = elem.parentNode;
		return parent && parent.nodeType !== 11 ? parent : null;
	},
	parents: function( elem ) {
		return jQuery.dir( elem, "parentNode" );
	},
	parentsUntil: function( elem, i, until ) {
		return jQuery.dir( elem, "parentNode", until );
	},
	next: function( elem ) {
		return sibling( elem, "nextSibling" );
	},
	prev: function( elem ) {
		return sibling( elem, "previousSibling" );
	},
	nextAll: function( elem ) {
		return jQuery.dir( elem, "nextSibling" );
	},
	prevAll: function( elem ) {
		return jQuery.dir( elem, "previousSibling" );
	},
	nextUntil: function( elem, i, until ) {
		return jQuery.dir( elem, "nextSibling", until );
	},
	prevUntil: function( elem, i, until ) {
		return jQuery.dir( elem, "previousSibling", until );
	},
	siblings: function( elem ) {
		return jQuery.sibling( ( elem.parentNode || {} ).firstChild, elem );
	},
	children: function( elem ) {
		return jQuery.sibling( elem.firstChild );
	},
	contents: function( elem ) {
		return elem.contentDocument || jQuery.merge( [], elem.childNodes );
	}
}, function( name, fn ) {
	jQuery.fn[ name ] = function( until, selector ) {
		var matched = jQuery.map( this, fn, until );

		if ( name.slice( -5 ) !== "Until" ) {
			selector = until;
		}

		if ( selector && typeof selector === "string" ) {
			matched = jQuery.filter( selector, matched );
		}

		if ( this.length > 1 ) {
			// Remove duplicates
			if ( !guaranteedUnique[ name ] ) {
				jQuery.unique( matched );
			}

			// Reverse order for parents* and prev-derivatives
			if ( rparentsprev.test( name ) ) {
				matched.reverse();
			}
		}

		return this.pushStack( matched );
	};
});
var rnotwhite = (/\S+/g);



// String to Object options format cache
var optionsCache = {};

// Convert String-formatted options into Object-formatted ones and store in cache
function createOptions( options ) {
	var object = optionsCache[ options ] = {};
	jQuery.each( options.match( rnotwhite ) || [], function( _, flag ) {
		object[ flag ] = true;
	});
	return object;
}

/*
 * Create a callback list using the following parameters:
 *
 *	options: an optional list of space-separated options that will change how
 *			the callback list behaves or a more traditional option object
 *
 * By default a callback list will act like an event callback list and can be
 * "fired" multiple times.
 *
 * Possible options:
 *
 *	once:			will ensure the callback list can only be fired once (like a Deferred)
 *
 *	memory:			will keep track of previous values and will call any callback added
 *					after the list has been fired right away with the latest "memorized"
 *					values (like a Deferred)
 *
 *	unique:			will ensure a callback can only be added once (no duplicate in the list)
 *
 *	stopOnFalse:	interrupt callings when a callback returns false
 *
 */
jQuery.Callbacks = function( options ) {

	// Convert options from String-formatted to Object-formatted if needed
	// (we check in cache first)
	options = typeof options === "string" ?
		( optionsCache[ options ] || createOptions( options ) ) :
		jQuery.extend( {}, options );

	var // Last fire value (for non-forgettable lists)
		memory,
		// Flag to know if list was already fired
		fired,
		// Flag to know if list is currently firing
		firing,
		// First callback to fire (used internally by add and fireWith)
		firingStart,
		// End of the loop when firing
		firingLength,
		// Index of currently firing callback (modified by remove if needed)
		firingIndex,
		// Actual callback list
		list = [],
		// Stack of fire calls for repeatable lists
		stack = !options.once && [],
		// Fire callbacks
		fire = function( data ) {
			memory = options.memory && data;
			fired = true;
			firingIndex = firingStart || 0;
			firingStart = 0;
			firingLength = list.length;
			firing = true;
			for ( ; list && firingIndex < firingLength; firingIndex++ ) {
				if ( list[ firingIndex ].apply( data[ 0 ], data[ 1 ] ) === false && options.stopOnFalse ) {
					memory = false; // To prevent further calls using add
					break;
				}
			}
			firing = false;
			if ( list ) {
				if ( stack ) {
					if ( stack.length ) {
						fire( stack.shift() );
					}
				} else if ( memory ) {
					list = [];
				} else {
					self.disable();
				}
			}
		},
		// Actual Callbacks object
		self = {
			// Add a callback or a collection of callbacks to the list
			add: function() {
				if ( list ) {
					// First, we save the current length
					var start = list.length;
					(function add( args ) {
						jQuery.each( args, function( _, arg ) {
							var type = jQuery.type( arg );
							if ( type === "function" ) {
								if ( !options.unique || !self.has( arg ) ) {
									list.push( arg );
								}
							} else if ( arg && arg.length && type !== "string" ) {
								// Inspect recursively
								add( arg );
							}
						});
					})( arguments );
					// Do we need to add the callbacks to the
					// current firing batch?
					if ( firing ) {
						firingLength = list.length;
					// With memory, if we're not firing then
					// we should call right away
					} else if ( memory ) {
						firingStart = start;
						fire( memory );
					}
				}
				return this;
			},
			// Remove a callback from the list
			remove: function() {
				if ( list ) {
					jQuery.each( arguments, function( _, arg ) {
						var index;
						while ( ( index = jQuery.inArray( arg, list, index ) ) > -1 ) {
							list.splice( index, 1 );
							// Handle firing indexes
							if ( firing ) {
								if ( index <= firingLength ) {
									firingLength--;
								}
								if ( index <= firingIndex ) {
									firingIndex--;
								}
							}
						}
					});
				}
				return this;
			},
			// Check if a given callback is in the list.
			// If no argument is given, return whether or not list has callbacks attached.
			has: function( fn ) {
				return fn ? jQuery.inArray( fn, list ) > -1 : !!( list && list.length );
			},
			// Remove all callbacks from the list
			empty: function() {
				list = [];
				firingLength = 0;
				return this;
			},
			// Have the list do nothing anymore
			disable: function() {
				list = stack = memory = undefined;
				return this;
			},
			// Is it disabled?
			disabled: function() {
				return !list;
			},
			// Lock the list in its current state
			lock: function() {
				stack = undefined;
				if ( !memory ) {
					self.disable();
				}
				return this;
			},
			// Is it locked?
			locked: function() {
				return !stack;
			},
			// Call all callbacks with the given context and arguments
			fireWith: function( context, args ) {
				if ( list && ( !fired || stack ) ) {
					args = args || [];
					args = [ context, args.slice ? args.slice() : args ];
					if ( firing ) {
						stack.push( args );
					} else {
						fire( args );
					}
				}
				return this;
			},
			// Call all the callbacks with the given arguments
			fire: function() {
				self.fireWith( this, arguments );
				return this;
			},
			// To know if the callbacks have already been called at least once
			fired: function() {
				return !!fired;
			}
		};

	return self;
};


jQuery.extend({

	Deferred: function( func ) {
		var tuples = [
				// action, add listener, listener list, final state
				[ "resolve", "done", jQuery.Callbacks("once memory"), "resolved" ],
				[ "reject", "fail", jQuery.Callbacks("once memory"), "rejected" ],
				[ "notify", "progress", jQuery.Callbacks("memory") ]
			],
			state = "pending",
			promise = {
				state: function() {
					return state;
				},
				always: function() {
					deferred.done( arguments ).fail( arguments );
					return this;
				},
				then: function( /* fnDone, fnFail, fnProgress */ ) {
					var fns = arguments;
					return jQuery.Deferred(function( newDefer ) {
						jQuery.each( tuples, function( i, tuple ) {
							var fn = jQuery.isFunction( fns[ i ] ) && fns[ i ];
							// deferred[ done | fail | progress ] for forwarding actions to newDefer
							deferred[ tuple[1] ](function() {
								var returned = fn && fn.apply( this, arguments );
								if ( returned && jQuery.isFunction( returned.promise ) ) {
									returned.promise()
										.done( newDefer.resolve )
										.fail( newDefer.reject )
										.progress( newDefer.notify );
								} else {
									newDefer[ tuple[ 0 ] + "With" ]( this === promise ? newDefer.promise() : this, fn ? [ returned ] : arguments );
								}
							});
						});
						fns = null;
					}).promise();
				},
				// Get a promise for this deferred
				// If obj is provided, the promise aspect is added to the object
				promise: function( obj ) {
					return obj != null ? jQuery.extend( obj, promise ) : promise;
				}
			},
			deferred = {};

		// Keep pipe for back-compat
		promise.pipe = promise.then;

		// Add list-specific methods
		jQuery.each( tuples, function( i, tuple ) {
			var list = tuple[ 2 ],
				stateString = tuple[ 3 ];

			// promise[ done | fail | progress ] = list.add
			promise[ tuple[1] ] = list.add;

			// Handle state
			if ( stateString ) {
				list.add(function() {
					// state = [ resolved | rejected ]
					state = stateString;

				// [ reject_list | resolve_list ].disable; progress_list.lock
				}, tuples[ i ^ 1 ][ 2 ].disable, tuples[ 2 ][ 2 ].lock );
			}

			// deferred[ resolve | reject | notify ]
			deferred[ tuple[0] ] = function() {
				deferred[ tuple[0] + "With" ]( this === deferred ? promise : this, arguments );
				return this;
			};
			deferred[ tuple[0] + "With" ] = list.fireWith;
		});

		// Make the deferred a promise
		promise.promise( deferred );

		// Call given func if any
		if ( func ) {
			func.call( deferred, deferred );
		}

		// All done!
		return deferred;
	},

	// Deferred helper
	when: function( subordinate /* , ..., subordinateN */ ) {
		var i = 0,
			resolveValues = slice.call( arguments ),
			length = resolveValues.length,

			// the count of uncompleted subordinates
			remaining = length !== 1 || ( subordinate && jQuery.isFunction( subordinate.promise ) ) ? length : 0,

			// the master Deferred. If resolveValues consist of only a single Deferred, just use that.
			deferred = remaining === 1 ? subordinate : jQuery.Deferred(),

			// Update function for both resolve and progress values
			updateFunc = function( i, contexts, values ) {
				return function( value ) {
					contexts[ i ] = this;
					values[ i ] = arguments.length > 1 ? slice.call( arguments ) : value;
					if ( values === progressValues ) {
						deferred.notifyWith( contexts, values );
					} else if ( !( --remaining ) ) {
						deferred.resolveWith( contexts, values );
					}
				};
			},

			progressValues, progressContexts, resolveContexts;

		// Add listeners to Deferred subordinates; treat others as resolved
		if ( length > 1 ) {
			progressValues = new Array( length );
			progressContexts = new Array( length );
			resolveContexts = new Array( length );
			for ( ; i < length; i++ ) {
				if ( resolveValues[ i ] && jQuery.isFunction( resolveValues[ i ].promise ) ) {
					resolveValues[ i ].promise()
						.done( updateFunc( i, resolveContexts, resolveValues ) )
						.fail( deferred.reject )
						.progress( updateFunc( i, progressContexts, progressValues ) );
				} else {
					--remaining;
				}
			}
		}

		// If we're not waiting on anything, resolve the master
		if ( !remaining ) {
			deferred.resolveWith( resolveContexts, resolveValues );
		}

		return deferred.promise();
	}
});


// The deferred used on DOM ready
var readyList;

jQuery.fn.ready = function( fn ) {
	// Add the callback
	jQuery.ready.promise().done( fn );

	return this;
};

jQuery.extend({
	// Is the DOM ready to be used? Set to true once it occurs.
	isReady: false,

	// A counter to track how many items to wait for before
	// the ready event fires. See #6781
	readyWait: 1,

	// Hold (or release) the ready event
	holdReady: function( hold ) {
		if ( hold ) {
			jQuery.readyWait++;
		} else {
			jQuery.ready( true );
		}
	},

	// Handle when the DOM is ready
	ready: function( wait ) {

		// Abort if there are pending holds or we're already ready
		if ( wait === true ? --jQuery.readyWait : jQuery.isReady ) {
			return;
		}

		// Remember that the DOM is ready
		jQuery.isReady = true;

		// If a normal DOM Ready event fired, decrement, and wait if need be
		if ( wait !== true && --jQuery.readyWait > 0 ) {
			return;
		}

		// If there are functions bound, to execute
		readyList.resolveWith( document, [ jQuery ] );

		// Trigger any bound ready events
		if ( jQuery.fn.triggerHandler ) {
			jQuery( document ).triggerHandler( "ready" );
			jQuery( document ).off( "ready" );
		}
	}
});

/**
 * The ready event handler and self cleanup method
 */
function completed() {
	document.removeEventListener( "DOMContentLoaded", completed, false );
	window.removeEventListener( "load", completed, false );
	jQuery.ready();
}

jQuery.ready.promise = function( obj ) {
	if ( !readyList ) {

		readyList = jQuery.Deferred();

		// Catch cases where $(document).ready() is called after the browser event has already occurred.
		// We once tried to use readyState "interactive" here, but it caused issues like the one
		// discovered by ChrisS here: http://bugs.jquery.com/ticket/12282#comment:15
		if ( document.readyState === "complete" ) {
			// Handle it asynchronously to allow scripts the opportunity to delay ready
			setTimeout( jQuery.ready );

		} else {

			// Use the handy event callback
			document.addEventListener( "DOMContentLoaded", completed, false );

			// A fallback to window.onload, that will always work
			window.addEventListener( "load", completed, false );
		}
	}
	return readyList.promise( obj );
};

// Kick off the DOM ready check even if the user does not
jQuery.ready.promise();




// Multifunctional method to get and set values of a collection
// The value/s can optionally be executed if it's a function
var access = jQuery.access = function( elems, fn, key, value, chainable, emptyGet, raw ) {
	var i = 0,
		len = elems.length,
		bulk = key == null;

	// Sets many values
	if ( jQuery.type( key ) === "object" ) {
		chainable = true;
		for ( i in key ) {
			jQuery.access( elems, fn, i, key[i], true, emptyGet, raw );
		}

	// Sets one value
	} else if ( value !== undefined ) {
		chainable = true;

		if ( !jQuery.isFunction( value ) ) {
			raw = true;
		}

		if ( bulk ) {
			// Bulk operations run against the entire set
			if ( raw ) {
				fn.call( elems, value );
				fn = null;

			// ...except when executing function values
			} else {
				bulk = fn;
				fn = function( elem, key, value ) {
					return bulk.call( jQuery( elem ), value );
				};
			}
		}

		if ( fn ) {
			for ( ; i < len; i++ ) {
				fn( elems[i], key, raw ? value : value.call( elems[i], i, fn( elems[i], key ) ) );
			}
		}
	}

	return chainable ?
		elems :

		// Gets
		bulk ?
			fn.call( elems ) :
			len ? fn( elems[0], key ) : emptyGet;
};


/**
 * Determines whether an object can have data
 */
jQuery.acceptData = function( owner ) {
	// Accepts only:
	//  - Node
	//    - Node.ELEMENT_NODE
	//    - Node.DOCUMENT_NODE
	//  - Object
	//    - Any
	/* jshint -W018 */
	return owner.nodeType === 1 || owner.nodeType === 9 || !( +owner.nodeType );
};


function Data() {
	// Support: Android<4,
	// Old WebKit does not have Object.preventExtensions/freeze method,
	// return new empty object instead with no [[set]] accessor
	Object.defineProperty( this.cache = {}, 0, {
		get: function() {
			return {};
		}
	});

	this.expando = jQuery.expando + Data.uid++;
}

Data.uid = 1;
Data.accepts = jQuery.acceptData;

Data.prototype = {
	key: function( owner ) {
		// We can accept data for non-element nodes in modern browsers,
		// but we should not, see #8335.
		// Always return the key for a frozen object.
		if ( !Data.accepts( owner ) ) {
			return 0;
		}

		var descriptor = {},
			// Check if the owner object already has a cache key
			unlock = owner[ this.expando ];

		// If not, create one
		if ( !unlock ) {
			unlock = Data.uid++;

			// Secure it in a non-enumerable, non-writable property
			try {
				descriptor[ this.expando ] = { value: unlock };
				Object.defineProperties( owner, descriptor );

			// Support: Android<4
			// Fallback to a less secure definition
			} catch ( e ) {
				descriptor[ this.expando ] = unlock;
				jQuery.extend( owner, descriptor );
			}
		}

		// Ensure the cache object
		if ( !this.cache[ unlock ] ) {
			this.cache[ unlock ] = {};
		}

		return unlock;
	},
	set: function( owner, data, value ) {
		var prop,
			// There may be an unlock assigned to this node,
			// if there is no entry for this "owner", create one inline
			// and set the unlock as though an owner entry had always existed
			unlock = this.key( owner ),
			cache = this.cache[ unlock ];

		// Handle: [ owner, key, value ] args
		if ( typeof data === "string" ) {
			cache[ data ] = value;

		// Handle: [ owner, { properties } ] args
		} else {
			// Fresh assignments by object are shallow copied
			if ( jQuery.isEmptyObject( cache ) ) {
				jQuery.extend( this.cache[ unlock ], data );
			// Otherwise, copy the properties one-by-one to the cache object
			} else {
				for ( prop in data ) {
					cache[ prop ] = data[ prop ];
				}
			}
		}
		return cache;
	},
	get: function( owner, key ) {
		// Either a valid cache is found, or will be created.
		// New caches will be created and the unlock returned,
		// allowing direct access to the newly created
		// empty data object. A valid owner object must be provided.
		var cache = this.cache[ this.key( owner ) ];

		return key === undefined ?
			cache : cache[ key ];
	},
	access: function( owner, key, value ) {
		var stored;
		// In cases where either:
		//
		//   1. No key was specified
		//   2. A string key was specified, but no value provided
		//
		// Take the "read" path and allow the get method to determine
		// which value to return, respectively either:
		//
		//   1. The entire cache object
		//   2. The data stored at the key
		//
		if ( key === undefined ||
				((key && typeof key === "string") && value === undefined) ) {

			stored = this.get( owner, key );

			return stored !== undefined ?
				stored : this.get( owner, jQuery.camelCase(key) );
		}

		// [*]When the key is not a string, or both a key and value
		// are specified, set or extend (existing objects) with either:
		//
		//   1. An object of properties
		//   2. A key and value
		//
		this.set( owner, key, value );

		// Since the "set" path can have two possible entry points
		// return the expected data based on which path was taken[*]
		return value !== undefined ? value : key;
	},
	remove: function( owner, key ) {
		var i, name, camel,
			unlock = this.key( owner ),
			cache = this.cache[ unlock ];

		if ( key === undefined ) {
			this.cache[ unlock ] = {};

		} else {
			// Support array or space separated string of keys
			if ( jQuery.isArray( key ) ) {
				// If "name" is an array of keys...
				// When data is initially created, via ("key", "val") signature,
				// keys will be converted to camelCase.
				// Since there is no way to tell _how_ a key was added, remove
				// both plain key and camelCase key. #12786
				// This will only penalize the array argument path.
				name = key.concat( key.map( jQuery.camelCase ) );
			} else {
				camel = jQuery.camelCase( key );
				// Try the string as a key before any manipulation
				if ( key in cache ) {
					name = [ key, camel ];
				} else {
					// If a key with the spaces exists, use it.
					// Otherwise, create an array by matching non-whitespace
					name = camel;
					name = name in cache ?
						[ name ] : ( name.match( rnotwhite ) || [] );
				}
			}

			i = name.length;
			while ( i-- ) {
				delete cache[ name[ i ] ];
			}
		}
	},
	hasData: function( owner ) {
		return !jQuery.isEmptyObject(
			this.cache[ owner[ this.expando ] ] || {}
		);
	},
	discard: function( owner ) {
		if ( owner[ this.expando ] ) {
			delete this.cache[ owner[ this.expando ] ];
		}
	}
};
var data_priv = new Data();

var data_user = new Data();



//	Implementation Summary
//
//	1. Enforce API surface and semantic compatibility with 1.9.x branch
//	2. Improve the module's maintainability by reducing the storage
//		paths to a single mechanism.
//	3. Use the same single mechanism to support "private" and "user" data.
//	4. _Never_ expose "private" data to user code (TODO: Drop _data, _removeData)
//	5. Avoid exposing implementation details on user objects (eg. expando properties)
//	6. Provide a clear path for implementation upgrade to WeakMap in 2014

var rbrace = /^(?:\{[\w\W]*\}|\[[\w\W]*\])$/,
	rmultiDash = /([A-Z])/g;

function dataAttr( elem, key, data ) {
	var name;

	// If nothing was found internally, try to fetch any
	// data from the HTML5 data-* attribute
	if ( data === undefined && elem.nodeType === 1 ) {
		name = "data-" + key.replace( rmultiDash, "-$1" ).toLowerCase();
		data = elem.getAttribute( name );

		if ( typeof data === "string" ) {
			try {
				data = data === "true" ? true :
					data === "false" ? false :
					data === "null" ? null :
					// Only convert to a number if it doesn't change the string
					+data + "" === data ? +data :
					rbrace.test( data ) ? jQuery.parseJSON( data ) :
					data;
			} catch( e ) {}

			// Make sure we set the data so it isn't changed later
			data_user.set( elem, key, data );
		} else {
			data = undefined;
		}
	}
	return data;
}

jQuery.extend({
	hasData: function( elem ) {
		return data_user.hasData( elem ) || data_priv.hasData( elem );
	},

	data: function( elem, name, data ) {
		return data_user.access( elem, name, data );
	},

	removeData: function( elem, name ) {
		data_user.remove( elem, name );
	},

	// TODO: Now that all calls to _data and _removeData have been replaced
	// with direct calls to data_priv methods, these can be deprecated.
	_data: function( elem, name, data ) {
		return data_priv.access( elem, name, data );
	},

	_removeData: function( elem, name ) {
		data_priv.remove( elem, name );
	}
});

jQuery.fn.extend({
	data: function( key, value ) {
		var i, name, data,
			elem = this[ 0 ],
			attrs = elem && elem.attributes;

		// Gets all values
		if ( key === undefined ) {
			if ( this.length ) {
				data = data_user.get( elem );

				if ( elem.nodeType === 1 && !data_priv.get( elem, "hasDataAttrs" ) ) {
					i = attrs.length;
					while ( i-- ) {

						// Support: IE11+
						// The attrs elements can be null (#14894)
						if ( attrs[ i ] ) {
							name = attrs[ i ].name;
							if ( name.indexOf( "data-" ) === 0 ) {
								name = jQuery.camelCase( name.slice(5) );
								dataAttr( elem, name, data[ name ] );
							}
						}
					}
					data_priv.set( elem, "hasDataAttrs", true );
				}
			}

			return data;
		}

		// Sets multiple values
		if ( typeof key === "object" ) {
			return this.each(function() {
				data_user.set( this, key );
			});
		}

		return access( this, function( value ) {
			var data,
				camelKey = jQuery.camelCase( key );

			// The calling jQuery object (element matches) is not empty
			// (and therefore has an element appears at this[ 0 ]) and the
			// `value` parameter was not undefined. An empty jQuery object
			// will result in `undefined` for elem = this[ 0 ] which will
			// throw an exception if an attempt to read a data cache is made.
			if ( elem && value === undefined ) {
				// Attempt to get data from the cache
				// with the key as-is
				data = data_user.get( elem, key );
				if ( data !== undefined ) {
					return data;
				}

				// Attempt to get data from the cache
				// with the key camelized
				data = data_user.get( elem, camelKey );
				if ( data !== undefined ) {
					return data;
				}

				// Attempt to "discover" the data in
				// HTML5 custom data-* attrs
				data = dataAttr( elem, camelKey, undefined );
				if ( data !== undefined ) {
					return data;
				}

				// We tried really hard, but the data doesn't exist.
				return;
			}

			// Set the data...
			this.each(function() {
				// First, attempt to store a copy or reference of any
				// data that might've been store with a camelCased key.
				var data = data_user.get( this, camelKey );

				// For HTML5 data-* attribute interop, we have to
				// store property names with dashes in a camelCase form.
				// This might not apply to all properties...*
				data_user.set( this, camelKey, value );

				// *... In the case of properties that might _actually_
				// have dashes, we need to also store a copy of that
				// unchanged property.
				if ( key.indexOf("-") !== -1 && data !== undefined ) {
					data_user.set( this, key, value );
				}
			});
		}, null, value, arguments.length > 1, null, true );
	},

	removeData: function( key ) {
		return this.each(function() {
			data_user.remove( this, key );
		});
	}
});


jQuery.extend({
	queue: function( elem, type, data ) {
		var queue;

		if ( elem ) {
			type = ( type || "fx" ) + "queue";
			queue = data_priv.get( elem, type );

			// Speed up dequeue by getting out quickly if this is just a lookup
			if ( data ) {
				if ( !queue || jQuery.isArray( data ) ) {
					queue = data_priv.access( elem, type, jQuery.makeArray(data) );
				} else {
					queue.push( data );
				}
			}
			return queue || [];
		}
	},

	dequeue: function( elem, type ) {
		type = type || "fx";

		var queue = jQuery.queue( elem, type ),
			startLength = queue.length,
			fn = queue.shift(),
			hooks = jQuery._queueHooks( elem, type ),
			next = function() {
				jQuery.dequeue( elem, type );
			};

		// If the fx queue is dequeued, always remove the progress sentinel
		if ( fn === "inprogress" ) {
			fn = queue.shift();
			startLength--;
		}

		if ( fn ) {

			// Add a progress sentinel to prevent the fx queue from being
			// automatically dequeued
			if ( type === "fx" ) {
				queue.unshift( "inprogress" );
			}

			// Clear up the last queue stop function
			delete hooks.stop;
			fn.call( elem, next, hooks );
		}

		if ( !startLength && hooks ) {
			hooks.empty.fire();
		}
	},

	// Not public - generate a queueHooks object, or return the current one
	_queueHooks: function( elem, type ) {
		var key = type + "queueHooks";
		return data_priv.get( elem, key ) || data_priv.access( elem, key, {
			empty: jQuery.Callbacks("once memory").add(function() {
				data_priv.remove( elem, [ type + "queue", key ] );
			})
		});
	}
});

jQuery.fn.extend({
	queue: function( type, data ) {
		var setter = 2;

		if ( typeof type !== "string" ) {
			data = type;
			type = "fx";
			setter--;
		}

		if ( arguments.length < setter ) {
			return jQuery.queue( this[0], type );
		}

		return data === undefined ?
			this :
			this.each(function() {
				var queue = jQuery.queue( this, type, data );

				// Ensure a hooks for this queue
				jQuery._queueHooks( this, type );

				if ( type === "fx" && queue[0] !== "inprogress" ) {
					jQuery.dequeue( this, type );
				}
			});
	},
	dequeue: function( type ) {
		return this.each(function() {
			jQuery.dequeue( this, type );
		});
	},
	clearQueue: function( type ) {
		return this.queue( type || "fx", [] );
	},
	// Get a promise resolved when queues of a certain type
	// are emptied (fx is the type by default)
	promise: function( type, obj ) {
		var tmp,
			count = 1,
			defer = jQuery.Deferred(),
			elements = this,
			i = this.length,
			resolve = function() {
				if ( !( --count ) ) {
					defer.resolveWith( elements, [ elements ] );
				}
			};

		if ( typeof type !== "string" ) {
			obj = type;
			type = undefined;
		}
		type = type || "fx";

		while ( i-- ) {
			tmp = data_priv.get( elements[ i ], type + "queueHooks" );
			if ( tmp && tmp.empty ) {
				count++;
				tmp.empty.add( resolve );
			}
		}
		resolve();
		return defer.promise( obj );
	}
});
var pnum = (/[+-]?(?:\d*\.|)\d+(?:[eE][+-]?\d+|)/).source;

var cssExpand = [ "Top", "Right", "Bottom", "Left" ];

var isHidden = function( elem, el ) {
		// isHidden might be called from jQuery#filter function;
		// in that case, element will be second argument
		elem = el || elem;
		return jQuery.css( elem, "display" ) === "none" || !jQuery.contains( elem.ownerDocument, elem );
	};

var rcheckableType = (/^(?:checkbox|radio)$/i);



(function() {
	var fragment = document.createDocumentFragment(),
		div = fragment.appendChild( document.createElement( "div" ) ),
		input = document.createElement( "input" );

	// Support: Safari<=5.1
	// Check state lost if the name is set (#11217)
	// Support: Windows Web Apps (WWA)
	// `name` and `type` must use .setAttribute for WWA (#14901)
	input.setAttribute( "type", "radio" );
	input.setAttribute( "checked", "checked" );
	input.setAttribute( "name", "t" );

	div.appendChild( input );

	// Support: Safari<=5.1, Android<4.2
	// Older WebKit doesn't clone checked state correctly in fragments
	support.checkClone = div.cloneNode( true ).cloneNode( true ).lastChild.checked;

	// Support: IE<=11+
	// Make sure textarea (and checkbox) defaultValue is properly cloned
	div.innerHTML = "<textarea>x</textarea>";
	support.noCloneChecked = !!div.cloneNode( true ).lastChild.defaultValue;
})();
var strundefined = typeof undefined;



support.focusinBubbles = "onfocusin" in window;


var
	rkeyEvent = /^key/,
	rmouseEvent = /^(?:mouse|pointer|contextmenu)|click/,
	rfocusMorph = /^(?:focusinfocus|focusoutblur)$/,
	rtypenamespace = /^([^.]*)(?:\.(.+)|)$/;

function returnTrue() {
	return true;
}

function returnFalse() {
	return false;
}

function safeActiveElement() {
	try {
		return document.activeElement;
	} catch ( err ) { }
}

/*
 * Helper functions for managing events -- not part of the public interface.
 * Props to Dean Edwards' addEvent library for many of the ideas.
 */
jQuery.event = {

	global: {},

	add: function( elem, types, handler, data, selector ) {

		var handleObjIn, eventHandle, tmp,
			events, t, handleObj,
			special, handlers, type, namespaces, origType,
			elemData = data_priv.get( elem );

		// Don't attach events to noData or text/comment nodes (but allow plain objects)
		if ( !elemData ) {
			return;
		}

		// Caller can pass in an object of custom data in lieu of the handler
		if ( handler.handler ) {
			handleObjIn = handler;
			handler = handleObjIn.handler;
			selector = handleObjIn.selector;
		}

		// Make sure that the handler has a unique ID, used to find/remove it later
		if ( !handler.guid ) {
			handler.guid = jQuery.guid++;
		}

		// Init the element's event structure and main handler, if this is the first
		if ( !(events = elemData.events) ) {
			events = elemData.events = {};
		}
		if ( !(eventHandle = elemData.handle) ) {
			eventHandle = elemData.handle = function( e ) {
				// Discard the second event of a jQuery.event.trigger() and
				// when an event is called after a page has unloaded
				return typeof jQuery !== strundefined && jQuery.event.triggered !== e.type ?
					jQuery.event.dispatch.apply( elem, arguments ) : undefined;
			};
		}

		// Handle multiple events separated by a space
		types = ( types || "" ).match( rnotwhite ) || [ "" ];
		t = types.length;
		while ( t-- ) {
			tmp = rtypenamespace.exec( types[t] ) || [];
			type = origType = tmp[1];
			namespaces = ( tmp[2] || "" ).split( "." ).sort();

			// There *must* be a type, no attaching namespace-only handlers
			if ( !type ) {
				continue;
			}

			// If event changes its type, use the special event handlers for the changed type
			special = jQuery.event.special[ type ] || {};

			// If selector defined, determine special event api type, otherwise given type
			type = ( selector ? special.delegateType : special.bindType ) || type;

			// Update special based on newly reset type
			special = jQuery.event.special[ type ] || {};

			// handleObj is passed to all event handlers
			handleObj = jQuery.extend({
				type: type,
				origType: origType,
				data: data,
				handler: handler,
				guid: handler.guid,
				selector: selector,
				needsContext: selector && jQuery.expr.match.needsContext.test( selector ),
				namespace: namespaces.join(".")
			}, handleObjIn );

			// Init the event handler queue if we're the first
			if ( !(handlers = events[ type ]) ) {
				handlers = events[ type ] = [];
				handlers.delegateCount = 0;

				// Only use addEventListener if the special events handler returns false
				if ( !special.setup || special.setup.call( elem, data, namespaces, eventHandle ) === false ) {
					if ( elem.addEventListener ) {
						elem.addEventListener( type, eventHandle, false );
					}
				}
			}

			if ( special.add ) {
				special.add.call( elem, handleObj );

				if ( !handleObj.handler.guid ) {
					handleObj.handler.guid = handler.guid;
				}
			}

			// Add to the element's handler list, delegates in front
			if ( selector ) {
				handlers.splice( handlers.delegateCount++, 0, handleObj );
			} else {
				handlers.push( handleObj );
			}

			// Keep track of which events have ever been used, for event optimization
			jQuery.event.global[ type ] = true;
		}

	},

	// Detach an event or set of events from an element
	remove: function( elem, types, handler, selector, mappedTypes ) {

		var j, origCount, tmp,
			events, t, handleObj,
			special, handlers, type, namespaces, origType,
			elemData = data_priv.hasData( elem ) && data_priv.get( elem );

		if ( !elemData || !(events = elemData.events) ) {
			return;
		}

		// Once for each type.namespace in types; type may be omitted
		types = ( types || "" ).match( rnotwhite ) || [ "" ];
		t = types.length;
		while ( t-- ) {
			tmp = rtypenamespace.exec( types[t] ) || [];
			type = origType = tmp[1];
			namespaces = ( tmp[2] || "" ).split( "." ).sort();

			// Unbind all events (on this namespace, if provided) for the element
			if ( !type ) {
				for ( type in events ) {
					jQuery.event.remove( elem, type + types[ t ], handler, selector, true );
				}
				continue;
			}

			special = jQuery.event.special[ type ] || {};
			type = ( selector ? special.delegateType : special.bindType ) || type;
			handlers = events[ type ] || [];
			tmp = tmp[2] && new RegExp( "(^|\\.)" + namespaces.join("\\.(?:.*\\.|)") + "(\\.|$)" );

			// Remove matching events
			origCount = j = handlers.length;
			while ( j-- ) {
				handleObj = handlers[ j ];

				if ( ( mappedTypes || origType === handleObj.origType ) &&
					( !handler || handler.guid === handleObj.guid ) &&
					( !tmp || tmp.test( handleObj.namespace ) ) &&
					( !selector || selector === handleObj.selector || selector === "**" && handleObj.selector ) ) {
					handlers.splice( j, 1 );

					if ( handleObj.selector ) {
						handlers.delegateCount--;
					}
					if ( special.remove ) {
						special.remove.call( elem, handleObj );
					}
				}
			}

			// Remove generic event handler if we removed something and no more handlers exist
			// (avoids potential for endless recursion during removal of special event handlers)
			if ( origCount && !handlers.length ) {
				if ( !special.teardown || special.teardown.call( elem, namespaces, elemData.handle ) === false ) {
					jQuery.removeEvent( elem, type, elemData.handle );
				}

				delete events[ type ];
			}
		}

		// Remove the expando if it's no longer used
		if ( jQuery.isEmptyObject( events ) ) {
			delete elemData.handle;
			data_priv.remove( elem, "events" );
		}
	},

	trigger: function( event, data, elem, onlyHandlers ) {

		var i, cur, tmp, bubbleType, ontype, handle, special,
			eventPath = [ elem || document ],
			type = hasOwn.call( event, "type" ) ? event.type : event,
			namespaces = hasOwn.call( event, "namespace" ) ? event.namespace.split(".") : [];

		cur = tmp = elem = elem || document;

		// Don't do events on text and comment nodes
		if ( elem.nodeType === 3 || elem.nodeType === 8 ) {
			return;
		}

		// focus/blur morphs to focusin/out; ensure we're not firing them right now
		if ( rfocusMorph.test( type + jQuery.event.triggered ) ) {
			return;
		}

		if ( type.indexOf(".") >= 0 ) {
			// Namespaced trigger; create a regexp to match event type in handle()
			namespaces = type.split(".");
			type = namespaces.shift();
			namespaces.sort();
		}
		ontype = type.indexOf(":") < 0 && "on" + type;

		// Caller can pass in a jQuery.Event object, Object, or just an event type string
		event = event[ jQuery.expando ] ?
			event :
			new jQuery.Event( type, typeof event === "object" && event );

		// Trigger bitmask: & 1 for native handlers; & 2 for jQuery (always true)
		event.isTrigger = onlyHandlers ? 2 : 3;
		event.namespace = namespaces.join(".");
		event.namespace_re = event.namespace ?
			new RegExp( "(^|\\.)" + namespaces.join("\\.(?:.*\\.|)") + "(\\.|$)" ) :
			null;

		// Clean up the event in case it is being reused
		event.result = undefined;
		if ( !event.target ) {
			event.target = elem;
		}

		// Clone any incoming data and prepend the event, creating the handler arg list
		data = data == null ?
			[ event ] :
			jQuery.makeArray( data, [ event ] );

		// Allow special events to draw outside the lines
		special = jQuery.event.special[ type ] || {};
		if ( !onlyHandlers && special.trigger && special.trigger.apply( elem, data ) === false ) {
			return;
		}

		// Determine event propagation path in advance, per W3C events spec (#9951)
		// Bubble up to document, then to window; watch for a global ownerDocument var (#9724)
		if ( !onlyHandlers && !special.noBubble && !jQuery.isWindow( elem ) ) {

			bubbleType = special.delegateType || type;
			if ( !rfocusMorph.test( bubbleType + type ) ) {
				cur = cur.parentNode;
			}
			for ( ; cur; cur = cur.parentNode ) {
				eventPath.push( cur );
				tmp = cur;
			}

			// Only add window if we got to document (e.g., not plain obj or detached DOM)
			if ( tmp === (elem.ownerDocument || document) ) {
				eventPath.push( tmp.defaultView || tmp.parentWindow || window );
			}
		}

		// Fire handlers on the event path
		i = 0;
		while ( (cur = eventPath[i++]) && !event.isPropagationStopped() ) {

			event.type = i > 1 ?
				bubbleType :
				special.bindType || type;

			// jQuery handler
			handle = ( data_priv.get( cur, "events" ) || {} )[ event.type ] && data_priv.get( cur, "handle" );
			if ( handle ) {
				handle.apply( cur, data );
			}

			// Native handler
			handle = ontype && cur[ ontype ];
			if ( handle && handle.apply && jQuery.acceptData( cur ) ) {
				event.result = handle.apply( cur, data );
				if ( event.result === false ) {
					event.preventDefault();
				}
			}
		}
		event.type = type;

		// If nobody prevented the default action, do it now
		if ( !onlyHandlers && !event.isDefaultPrevented() ) {

			if ( (!special._default || special._default.apply( eventPath.pop(), data ) === false) &&
				jQuery.acceptData( elem ) ) {

				// Call a native DOM method on the target with the same name name as the event.
				// Don't do default actions on window, that's where global variables be (#6170)
				if ( ontype && jQuery.isFunction( elem[ type ] ) && !jQuery.isWindow( elem ) ) {

					// Don't re-trigger an onFOO event when we call its FOO() method
					tmp = elem[ ontype ];

					if ( tmp ) {
						elem[ ontype ] = null;
					}

					// Prevent re-triggering of the same event, since we already bubbled it above
					jQuery.event.triggered = type;
					elem[ type ]();
					jQuery.event.triggered = undefined;

					if ( tmp ) {
						elem[ ontype ] = tmp;
					}
				}
			}
		}

		return event.result;
	},

	dispatch: function( event ) {

		// Make a writable jQuery.Event from the native event object
		event = jQuery.event.fix( event );

		var i, j, ret, matched, handleObj,
			handlerQueue = [],
			args = slice.call( arguments ),
			handlers = ( data_priv.get( this, "events" ) || {} )[ event.type ] || [],
			special = jQuery.event.special[ event.type ] || {};

		// Use the fix-ed jQuery.Event rather than the (read-only) native event
		args[0] = event;
		event.delegateTarget = this;

		// Call the preDispatch hook for the mapped type, and let it bail if desired
		if ( special.preDispatch && special.preDispatch.call( this, event ) === false ) {
			return;
		}

		// Determine handlers
		handlerQueue = jQuery.event.handlers.call( this, event, handlers );

		// Run delegates first; they may want to stop propagation beneath us
		i = 0;
		while ( (matched = handlerQueue[ i++ ]) && !event.isPropagationStopped() ) {
			event.currentTarget = matched.elem;

			j = 0;
			while ( (handleObj = matched.handlers[ j++ ]) && !event.isImmediatePropagationStopped() ) {

				// Triggered event must either 1) have no namespace, or 2) have namespace(s)
				// a subset or equal to those in the bound event (both can have no namespace).
				if ( !event.namespace_re || event.namespace_re.test( handleObj.namespace ) ) {

					event.handleObj = handleObj;
					event.data = handleObj.data;

					ret = ( (jQuery.event.special[ handleObj.origType ] || {}).handle || handleObj.handler )
							.apply( matched.elem, args );

					if ( ret !== undefined ) {
						if ( (event.result = ret) === false ) {
							event.preventDefault();
							event.stopPropagation();
						}
					}
				}
			}
		}

		// Call the postDispatch hook for the mapped type
		if ( special.postDispatch ) {
			special.postDispatch.call( this, event );
		}

		return event.result;
	},

	handlers: function( event, handlers ) {
		var i, matches, sel, handleObj,
			handlerQueue = [],
			delegateCount = handlers.delegateCount,
			cur = event.target;

		// Find delegate handlers
		// Black-hole SVG <use> instance trees (#13180)
		// Avoid non-left-click bubbling in Firefox (#3861)
		if ( delegateCount && cur.nodeType && (!event.button || event.type !== "click") ) {

			for ( ; cur !== this; cur = cur.parentNode || this ) {

				// Don't process clicks on disabled elements (#6911, #8165, #11382, #11764)
				if ( cur.disabled !== true || event.type !== "click" ) {
					matches = [];
					for ( i = 0; i < delegateCount; i++ ) {
						handleObj = handlers[ i ];

						// Don't conflict with Object.prototype properties (#13203)
						sel = handleObj.selector + " ";

						if ( matches[ sel ] === undefined ) {
							matches[ sel ] = handleObj.needsContext ?
								jQuery( sel, this ).index( cur ) >= 0 :
								jQuery.find( sel, this, null, [ cur ] ).length;
						}
						if ( matches[ sel ] ) {
							matches.push( handleObj );
						}
					}
					if ( matches.length ) {
						handlerQueue.push({ elem: cur, handlers: matches });
					}
				}
			}
		}

		// Add the remaining (directly-bound) handlers
		if ( delegateCount < handlers.length ) {
			handlerQueue.push({ elem: this, handlers: handlers.slice( delegateCount ) });
		}

		return handlerQueue;
	},

	// Includes some event props shared by KeyEvent and MouseEvent
	props: "altKey bubbles cancelable ctrlKey currentTarget eventPhase metaKey relatedTarget shiftKey target timeStamp view which".split(" "),

	fixHooks: {},

	keyHooks: {
		props: "char charCode key keyCode".split(" "),
		filter: function( event, original ) {

			// Add which for key events
			if ( event.which == null ) {
				event.which = original.charCode != null ? original.charCode : original.keyCode;
			}

			return event;
		}
	},

	mouseHooks: {
		props: "button buttons clientX clientY offsetX offsetY pageX pageY screenX screenY toElement".split(" "),
		filter: function( event, original ) {
			var eventDoc, doc, body,
				button = original.button;

			// Calculate pageX/Y if missing and clientX/Y available
			if ( event.pageX == null && original.clientX != null ) {
				eventDoc = event.target.ownerDocument || document;
				doc = eventDoc.documentElement;
				body = eventDoc.body;

				event.pageX = original.clientX + ( doc && doc.scrollLeft || body && body.scrollLeft || 0 ) - ( doc && doc.clientLeft || body && body.clientLeft || 0 );
				event.pageY = original.clientY + ( doc && doc.scrollTop  || body && body.scrollTop  || 0 ) - ( doc && doc.clientTop  || body && body.clientTop  || 0 );
			}

			// Add which for click: 1 === left; 2 === middle; 3 === right
			// Note: button is not normalized, so don't use it
			if ( !event.which && button !== undefined ) {
				event.which = ( button & 1 ? 1 : ( button & 2 ? 3 : ( button & 4 ? 2 : 0 ) ) );
			}

			return event;
		}
	},

	fix: function( event ) {
		if ( event[ jQuery.expando ] ) {
			return event;
		}

		// Create a writable copy of the event object and normalize some properties
		var i, prop, copy,
			type = event.type,
			originalEvent = event,
			fixHook = this.fixHooks[ type ];

		if ( !fixHook ) {
			this.fixHooks[ type ] = fixHook =
				rmouseEvent.test( type ) ? this.mouseHooks :
				rkeyEvent.test( type ) ? this.keyHooks :
				{};
		}
		copy = fixHook.props ? this.props.concat( fixHook.props ) : this.props;

		event = new jQuery.Event( originalEvent );

		i = copy.length;
		while ( i-- ) {
			prop = copy[ i ];
			event[ prop ] = originalEvent[ prop ];
		}

		// Support: Cordova 2.5 (WebKit) (#13255)
		// All events should have a target; Cordova deviceready doesn't
		if ( !event.target ) {
			event.target = document;
		}

		// Support: Safari 6.0+, Chrome<28
		// Target should not be a text node (#504, #13143)
		if ( event.target.nodeType === 3 ) {
			event.target = event.target.parentNode;
		}

		return fixHook.filter ? fixHook.filter( event, originalEvent ) : event;
	},

	special: {
		load: {
			// Prevent triggered image.load events from bubbling to window.load
			noBubble: true
		},
		focus: {
			// Fire native event if possible so blur/focus sequence is correct
			trigger: function() {
				if ( this !== safeActiveElement() && this.focus ) {
					this.focus();
					return false;
				}
			},
			delegateType: "focusin"
		},
		blur: {
			trigger: function() {
				if ( this === safeActiveElement() && this.blur ) {
					this.blur();
					return false;
				}
			},
			delegateType: "focusout"
		},
		click: {
			// For checkbox, fire native event so checked state will be right
			trigger: function() {
				if ( this.type === "checkbox" && this.click && jQuery.nodeName( this, "input" ) ) {
					this.click();
					return false;
				}
			},

			// For cross-browser consistency, don't fire native .click() on links
			_default: function( event ) {
				return jQuery.nodeName( event.target, "a" );
			}
		},

		beforeunload: {
			postDispatch: function( event ) {

				// Support: Firefox 20+
				// Firefox doesn't alert if the returnValue field is not set.
				if ( event.result !== undefined && event.originalEvent ) {
					event.originalEvent.returnValue = event.result;
				}
			}
		}
	},

	simulate: function( type, elem, event, bubble ) {
		// Piggyback on a donor event to simulate a different one.
		// Fake originalEvent to avoid donor's stopPropagation, but if the
		// simulated event prevents default then we do the same on the donor.
		var e = jQuery.extend(
			new jQuery.Event(),
			event,
			{
				type: type,
				isSimulated: true,
				originalEvent: {}
			}
		);
		if ( bubble ) {
			jQuery.event.trigger( e, null, elem );
		} else {
			jQuery.event.dispatch.call( elem, e );
		}
		if ( e.isDefaultPrevented() ) {
			event.preventDefault();
		}
	}
};

jQuery.removeEvent = function( elem, type, handle ) {
	if ( elem.removeEventListener ) {
		elem.removeEventListener( type, handle, false );
	}
};

jQuery.Event = function( src, props ) {
	// Allow instantiation without the 'new' keyword
	if ( !(this instanceof jQuery.Event) ) {
		return new jQuery.Event( src, props );
	}

	// Event object
	if ( src && src.type ) {
		this.originalEvent = src;
		this.type = src.type;

		// Events bubbling up the document may have been marked as prevented
		// by a handler lower down the tree; reflect the correct value.
		this.isDefaultPrevented = src.defaultPrevented ||
				src.defaultPrevented === undefined &&
				// Support: Android<4.0
				src.returnValue === false ?
			returnTrue :
			returnFalse;

	// Event type
	} else {
		this.type = src;
	}

	// Put explicitly provided properties onto the event object
	if ( props ) {
		jQuery.extend( this, props );
	}

	// Create a timestamp if incoming event doesn't have one
	this.timeStamp = src && src.timeStamp || jQuery.now();

	// Mark it as fixed
	this[ jQuery.expando ] = true;
};

// jQuery.Event is based on DOM3 Events as specified by the ECMAScript Language Binding
// http://www.w3.org/TR/2003/WD-DOM-Level-3-Events-20030331/ecma-script-binding.html
jQuery.Event.prototype = {
	isDefaultPrevented: returnFalse,
	isPropagationStopped: returnFalse,
	isImmediatePropagationStopped: returnFalse,

	preventDefault: function() {
		var e = this.originalEvent;

		this.isDefaultPrevented = returnTrue;

		if ( e && e.preventDefault ) {
			e.preventDefault();
		}
	},
	stopPropagation: function() {
		var e = this.originalEvent;

		this.isPropagationStopped = returnTrue;

		if ( e && e.stopPropagation ) {
			e.stopPropagation();
		}
	},
	stopImmediatePropagation: function() {
		var e = this.originalEvent;

		this.isImmediatePropagationStopped = returnTrue;

		if ( e && e.stopImmediatePropagation ) {
			e.stopImmediatePropagation();
		}

		this.stopPropagation();
	}
};

// Create mouseenter/leave events using mouseover/out and event-time checks
// Support: Chrome 15+
jQuery.each({
	mouseenter: "mouseover",
	mouseleave: "mouseout",
	pointerenter: "pointerover",
	pointerleave: "pointerout"
}, function( orig, fix ) {
	jQuery.event.special[ orig ] = {
		delegateType: fix,
		bindType: fix,

		handle: function( event ) {
			var ret,
				target = this,
				related = event.relatedTarget,
				handleObj = event.handleObj;

			// For mousenter/leave call the handler if related is outside the target.
			// NB: No relatedTarget if the mouse left/entered the browser window
			if ( !related || (related !== target && !jQuery.contains( target, related )) ) {
				event.type = handleObj.origType;
				ret = handleObj.handler.apply( this, arguments );
				event.type = fix;
			}
			return ret;
		}
	};
});

// Support: Firefox, Chrome, Safari
// Create "bubbling" focus and blur events
if ( !support.focusinBubbles ) {
	jQuery.each({ focus: "focusin", blur: "focusout" }, function( orig, fix ) {

		// Attach a single capturing handler on the document while someone wants focusin/focusout
		var handler = function( event ) {
				jQuery.event.simulate( fix, event.target, jQuery.event.fix( event ), true );
			};

		jQuery.event.special[ fix ] = {
			setup: function() {
				var doc = this.ownerDocument || this,
					attaches = data_priv.access( doc, fix );

				if ( !attaches ) {
					doc.addEventListener( orig, handler, true );
				}
				data_priv.access( doc, fix, ( attaches || 0 ) + 1 );
			},
			teardown: function() {
				var doc = this.ownerDocument || this,
					attaches = data_priv.access( doc, fix ) - 1;

				if ( !attaches ) {
					doc.removeEventListener( orig, handler, true );
					data_priv.remove( doc, fix );

				} else {
					data_priv.access( doc, fix, attaches );
				}
			}
		};
	});
}

jQuery.fn.extend({

	on: function( types, selector, data, fn, /*INTERNAL*/ one ) {
		var origFn, type;

		// Types can be a map of types/handlers
		if ( typeof types === "object" ) {
			// ( types-Object, selector, data )
			if ( typeof selector !== "string" ) {
				// ( types-Object, data )
				data = data || selector;
				selector = undefined;
			}
			for ( type in types ) {
				this.on( type, selector, data, types[ type ], one );
			}
			return this;
		}

		if ( data == null && fn == null ) {
			// ( types, fn )
			fn = selector;
			data = selector = undefined;
		} else if ( fn == null ) {
			if ( typeof selector === "string" ) {
				// ( types, selector, fn )
				fn = data;
				data = undefined;
			} else {
				// ( types, data, fn )
				fn = data;
				data = selector;
				selector = undefined;
			}
		}
		if ( fn === false ) {
			fn = returnFalse;
		} else if ( !fn ) {
			return this;
		}

		if ( one === 1 ) {
			origFn = fn;
			fn = function( event ) {
				// Can use an empty set, since event contains the info
				jQuery().off( event );
				return origFn.apply( this, arguments );
			};
			// Use same guid so caller can remove using origFn
			fn.guid = origFn.guid || ( origFn.guid = jQuery.guid++ );
		}
		return this.each( function() {
			jQuery.event.add( this, types, fn, data, selector );
		});
	},
	one: function( types, selector, data, fn ) {
		return this.on( types, selector, data, fn, 1 );
	},
	off: function( types, selector, fn ) {
		var handleObj, type;
		if ( types && types.preventDefault && types.handleObj ) {
			// ( event )  dispatched jQuery.Event
			handleObj = types.handleObj;
			jQuery( types.delegateTarget ).off(
				handleObj.namespace ? handleObj.origType + "." + handleObj.namespace : handleObj.origType,
				handleObj.selector,
				handleObj.handler
			);
			return this;
		}
		if ( typeof types === "object" ) {
			// ( types-object [, selector] )
			for ( type in types ) {
				this.off( type, selector, types[ type ] );
			}
			return this;
		}
		if ( selector === false || typeof selector === "function" ) {
			// ( types [, fn] )
			fn = selector;
			selector = undefined;
		}
		if ( fn === false ) {
			fn = returnFalse;
		}
		return this.each(function() {
			jQuery.event.remove( this, types, fn, selector );
		});
	},

	trigger: function( type, data ) {
		return this.each(function() {
			jQuery.event.trigger( type, data, this );
		});
	},
	triggerHandler: function( type, data ) {
		var elem = this[0];
		if ( elem ) {
			return jQuery.event.trigger( type, data, elem, true );
		}
	}
});


var
	rxhtmlTag = /<(?!area|br|col|embed|hr|img|input|link|meta|param)(([\w:]+)[^>]*)\/>/gi,
	rtagName = /<([\w:]+)/,
	rhtml = /<|&#?\w+;/,
	rnoInnerhtml = /<(?:script|style|link)/i,
	// checked="checked" or checked
	rchecked = /checked\s*(?:[^=]|=\s*.checked.)/i,
	rscriptType = /^$|\/(?:java|ecma)script/i,
	rscriptTypeMasked = /^true\/(.*)/,
	rcleanScript = /^\s*<!(?:\[CDATA\[|--)|(?:\]\]|--)>\s*$/g,

	// We have to close these tags to support XHTML (#13200)
	wrapMap = {

		// Support: IE9
		option: [ 1, "<select multiple='multiple'>", "</select>" ],

		thead: [ 1, "<table>", "</table>" ],
		col: [ 2, "<table><colgroup>", "</colgroup></table>" ],
		tr: [ 2, "<table><tbody>", "</tbody></table>" ],
		td: [ 3, "<table><tbody><tr>", "</tr></tbody></table>" ],

		_default: [ 0, "", "" ]
	};

// Support: IE9
wrapMap.optgroup = wrapMap.option;

wrapMap.tbody = wrapMap.tfoot = wrapMap.colgroup = wrapMap.caption = wrapMap.thead;
wrapMap.th = wrapMap.td;

// Support: 1.x compatibility
// Manipulating tables requires a tbody
function manipulationTarget( elem, content ) {
	return jQuery.nodeName( elem, "table" ) &&
		jQuery.nodeName( content.nodeType !== 11 ? content : content.firstChild, "tr" ) ?

		elem.getElementsByTagName("tbody")[0] ||
			elem.appendChild( elem.ownerDocument.createElement("tbody") ) :
		elem;
}

// Replace/restore the type attribute of script elements for safe DOM manipulation
function disableScript( elem ) {
	elem.type = (elem.getAttribute("type") !== null) + "/" + elem.type;
	return elem;
}
function restoreScript( elem ) {
	var match = rscriptTypeMasked.exec( elem.type );

	if ( match ) {
		elem.type = match[ 1 ];
	} else {
		elem.removeAttribute("type");
	}

	return elem;
}

// Mark scripts as having already been evaluated
function setGlobalEval( elems, refElements ) {
	var i = 0,
		l = elems.length;

	for ( ; i < l; i++ ) {
		data_priv.set(
			elems[ i ], "globalEval", !refElements || data_priv.get( refElements[ i ], "globalEval" )
		);
	}
}

function cloneCopyEvent( src, dest ) {
	var i, l, type, pdataOld, pdataCur, udataOld, udataCur, events;

	if ( dest.nodeType !== 1 ) {
		return;
	}

	// 1. Copy private data: events, handlers, etc.
	if ( data_priv.hasData( src ) ) {
		pdataOld = data_priv.access( src );
		pdataCur = data_priv.set( dest, pdataOld );
		events = pdataOld.events;

		if ( events ) {
			delete pdataCur.handle;
			pdataCur.events = {};

			for ( type in events ) {
				for ( i = 0, l = events[ type ].length; i < l; i++ ) {
					jQuery.event.add( dest, type, events[ type ][ i ] );
				}
			}
		}
	}

	// 2. Copy user data
	if ( data_user.hasData( src ) ) {
		udataOld = data_user.access( src );
		udataCur = jQuery.extend( {}, udataOld );

		data_user.set( dest, udataCur );
	}
}

function getAll( context, tag ) {
	var ret = context.getElementsByTagName ? context.getElementsByTagName( tag || "*" ) :
			context.querySelectorAll ? context.querySelectorAll( tag || "*" ) :
			[];

	return tag === undefined || tag && jQuery.nodeName( context, tag ) ?
		jQuery.merge( [ context ], ret ) :
		ret;
}

// Fix IE bugs, see support tests
function fixInput( src, dest ) {
	var nodeName = dest.nodeName.toLowerCase();

	// Fails to persist the checked state of a cloned checkbox or radio button.
	if ( nodeName === "input" && rcheckableType.test( src.type ) ) {
		dest.checked = src.checked;

	// Fails to return the selected option to the default selected state when cloning options
	} else if ( nodeName === "input" || nodeName === "textarea" ) {
		dest.defaultValue = src.defaultValue;
	}
}

jQuery.extend({
	clone: function( elem, dataAndEvents, deepDataAndEvents ) {
		var i, l, srcElements, destElements,
			clone = elem.cloneNode( true ),
			inPage = jQuery.contains( elem.ownerDocument, elem );

		// Fix IE cloning issues
		if ( !support.noCloneChecked && ( elem.nodeType === 1 || elem.nodeType === 11 ) &&
				!jQuery.isXMLDoc( elem ) ) {

			// We eschew Sizzle here for performance reasons: http://jsperf.com/getall-vs-sizzle/2
			destElements = getAll( clone );
			srcElements = getAll( elem );

			for ( i = 0, l = srcElements.length; i < l; i++ ) {
				fixInput( srcElements[ i ], destElements[ i ] );
			}
		}

		// Copy the events from the original to the clone
		if ( dataAndEvents ) {
			if ( deepDataAndEvents ) {
				srcElements = srcElements || getAll( elem );
				destElements = destElements || getAll( clone );

				for ( i = 0, l = srcElements.length; i < l; i++ ) {
					cloneCopyEvent( srcElements[ i ], destElements[ i ] );
				}
			} else {
				cloneCopyEvent( elem, clone );
			}
		}

		// Preserve script evaluation history
		destElements = getAll( clone, "script" );
		if ( destElements.length > 0 ) {
			setGlobalEval( destElements, !inPage && getAll( elem, "script" ) );
		}

		// Return the cloned set
		return clone;
	},

	buildFragment: function( elems, context, scripts, selection ) {
		var elem, tmp, tag, wrap, contains, j,
			fragment = context.createDocumentFragment(),
			nodes = [],
			i = 0,
			l = elems.length;

		for ( ; i < l; i++ ) {
			elem = elems[ i ];

			if ( elem || elem === 0 ) {

				// Add nodes directly
				if ( jQuery.type( elem ) === "object" ) {
					// Support: QtWebKit, PhantomJS
					// push.apply(_, arraylike) throws on ancient WebKit
					jQuery.merge( nodes, elem.nodeType ? [ elem ] : elem );

				// Convert non-html into a text node
				} else if ( !rhtml.test( elem ) ) {
					nodes.push( context.createTextNode( elem ) );

				// Convert html into DOM nodes
				} else {
					tmp = tmp || fragment.appendChild( context.createElement("div") );

					// Deserialize a standard representation
					tag = ( rtagName.exec( elem ) || [ "", "" ] )[ 1 ].toLowerCase();
					wrap = wrapMap[ tag ] || wrapMap._default;
					tmp.innerHTML = wrap[ 1 ] + elem.replace( rxhtmlTag, "<$1></$2>" ) + wrap[ 2 ];

					// Descend through wrappers to the right content
					j = wrap[ 0 ];
					while ( j-- ) {
						tmp = tmp.lastChild;
					}

					// Support: QtWebKit, PhantomJS
					// push.apply(_, arraylike) throws on ancient WebKit
					jQuery.merge( nodes, tmp.childNodes );

					// Remember the top-level container
					tmp = fragment.firstChild;

					// Ensure the created nodes are orphaned (#12392)
					tmp.textContent = "";
				}
			}
		}

		// Remove wrapper from fragment
		fragment.textContent = "";

		i = 0;
		while ( (elem = nodes[ i++ ]) ) {

			// #4087 - If origin and destination elements are the same, and this is
			// that element, do not do anything
			if ( selection && jQuery.inArray( elem, selection ) !== -1 ) {
				continue;
			}

			contains = jQuery.contains( elem.ownerDocument, elem );

			// Append to fragment
			tmp = getAll( fragment.appendChild( elem ), "script" );

			// Preserve script evaluation history
			if ( contains ) {
				setGlobalEval( tmp );
			}

			// Capture executables
			if ( scripts ) {
				j = 0;
				while ( (elem = tmp[ j++ ]) ) {
					if ( rscriptType.test( elem.type || "" ) ) {
						scripts.push( elem );
					}
				}
			}
		}

		return fragment;
	},

	cleanData: function( elems ) {
		var data, elem, type, key,
			special = jQuery.event.special,
			i = 0;

		for ( ; (elem = elems[ i ]) !== undefined; i++ ) {
			if ( jQuery.acceptData( elem ) ) {
				key = elem[ data_priv.expando ];

				if ( key && (data = data_priv.cache[ key ]) ) {
					if ( data.events ) {
						for ( type in data.events ) {
							if ( special[ type ] ) {
								jQuery.event.remove( elem, type );

							// This is a shortcut to avoid jQuery.event.remove's overhead
							} else {
								jQuery.removeEvent( elem, type, data.handle );
							}
						}
					}
					if ( data_priv.cache[ key ] ) {
						// Discard any remaining `private` data
						delete data_priv.cache[ key ];
					}
				}
			}
			// Discard any remaining `user` data
			delete data_user.cache[ elem[ data_user.expando ] ];
		}
	}
});

jQuery.fn.extend({
	text: function( value ) {
		return access( this, function( value ) {
			return value === undefined ?
				jQuery.text( this ) :
				this.empty().each(function() {
					if ( this.nodeType === 1 || this.nodeType === 11 || this.nodeType === 9 ) {
						this.textContent = value;
					}
				});
		}, null, value, arguments.length );
	},

	append: function() {
		return this.domManip( arguments, function( elem ) {
			if ( this.nodeType === 1 || this.nodeType === 11 || this.nodeType === 9 ) {
				var target = manipulationTarget( this, elem );
				target.appendChild( elem );
			}
		});
	},

	prepend: function() {
		return this.domManip( arguments, function( elem ) {
			if ( this.nodeType === 1 || this.nodeType === 11 || this.nodeType === 9 ) {
				var target = manipulationTarget( this, elem );
				target.insertBefore( elem, target.firstChild );
			}
		});
	},

	before: function() {
		return this.domManip( arguments, function( elem ) {
			if ( this.parentNode ) {
				this.parentNode.insertBefore( elem, this );
			}
		});
	},

	after: function() {
		return this.domManip( arguments, function( elem ) {
			if ( this.parentNode ) {
				this.parentNode.insertBefore( elem, this.nextSibling );
			}
		});
	},

	remove: function( selector, keepData /* Internal Use Only */ ) {
		var elem,
			elems = selector ? jQuery.filter( selector, this ) : this,
			i = 0;

		for ( ; (elem = elems[i]) != null; i++ ) {
			if ( !keepData && elem.nodeType === 1 ) {
				jQuery.cleanData( getAll( elem ) );
			}

			if ( elem.parentNode ) {
				if ( keepData && jQuery.contains( elem.ownerDocument, elem ) ) {
					setGlobalEval( getAll( elem, "script" ) );
				}
				elem.parentNode.removeChild( elem );
			}
		}

		return this;
	},

	empty: function() {
		var elem,
			i = 0;

		for ( ; (elem = this[i]) != null; i++ ) {
			if ( elem.nodeType === 1 ) {

				// Prevent memory leaks
				jQuery.cleanData( getAll( elem, false ) );

				// Remove any remaining nodes
				elem.textContent = "";
			}
		}

		return this;
	},

	clone: function( dataAndEvents, deepDataAndEvents ) {
		dataAndEvents = dataAndEvents == null ? false : dataAndEvents;
		deepDataAndEvents = deepDataAndEvents == null ? dataAndEvents : deepDataAndEvents;

		return this.map(function() {
			return jQuery.clone( this, dataAndEvents, deepDataAndEvents );
		});
	},

	html: function( value ) {
		return access( this, function( value ) {
			var elem = this[ 0 ] || {},
				i = 0,
				l = this.length;

			if ( value === undefined && elem.nodeType === 1 ) {
				return elem.innerHTML;
			}

			// See if we can take a shortcut and just use innerHTML
			if ( typeof value === "string" && !rnoInnerhtml.test( value ) &&
				!wrapMap[ ( rtagName.exec( value ) || [ "", "" ] )[ 1 ].toLowerCase() ] ) {

				value = value.replace( rxhtmlTag, "<$1></$2>" );

				try {
					for ( ; i < l; i++ ) {
						elem = this[ i ] || {};

						// Remove element nodes and prevent memory leaks
						if ( elem.nodeType === 1 ) {
							jQuery.cleanData( getAll( elem, false ) );
							elem.innerHTML = value;
						}
					}

					elem = 0;

				// If using innerHTML throws an exception, use the fallback method
				} catch( e ) {}
			}

			if ( elem ) {
				this.empty().append( value );
			}
		}, null, value, arguments.length );
	},

	replaceWith: function() {
		var arg = arguments[ 0 ];

		// Make the changes, replacing each context element with the new content
		this.domManip( arguments, function( elem ) {
			arg = this.parentNode;

			jQuery.cleanData( getAll( this ) );

			if ( arg ) {
				arg.replaceChild( elem, this );
			}
		});

		// Force removal if there was no new content (e.g., from empty arguments)
		return arg && (arg.length || arg.nodeType) ? this : this.remove();
	},

	detach: function( selector ) {
		return this.remove( selector, true );
	},

	domManip: function( args, callback ) {

		// Flatten any nested arrays
		args = concat.apply( [], args );

		var fragment, first, scripts, hasScripts, node, doc,
			i = 0,
			l = this.length,
			set = this,
			iNoClone = l - 1,
			value = args[ 0 ],
			isFunction = jQuery.isFunction( value );

		// We can't cloneNode fragments that contain checked, in WebKit
		if ( isFunction ||
				( l > 1 && typeof value === "string" &&
					!support.checkClone && rchecked.test( value ) ) ) {
			return this.each(function( index ) {
				var self = set.eq( index );
				if ( isFunction ) {
					args[ 0 ] = value.call( this, index, self.html() );
				}
				self.domManip( args, callback );
			});
		}

		if ( l ) {
			fragment = jQuery.buildFragment( args, this[ 0 ].ownerDocument, false, this );
			first = fragment.firstChild;

			if ( fragment.childNodes.length === 1 ) {
				fragment = first;
			}

			if ( first ) {
				scripts = jQuery.map( getAll( fragment, "script" ), disableScript );
				hasScripts = scripts.length;

				// Use the original fragment for the last item instead of the first because it can end up
				// being emptied incorrectly in certain situations (#8070).
				for ( ; i < l; i++ ) {
					node = fragment;

					if ( i !== iNoClone ) {
						node = jQuery.clone( node, true, true );

						// Keep references to cloned scripts for later restoration
						if ( hasScripts ) {
							// Support: QtWebKit
							// jQuery.merge because push.apply(_, arraylike) throws
							jQuery.merge( scripts, getAll( node, "script" ) );
						}
					}

					callback.call( this[ i ], node, i );
				}

				if ( hasScripts ) {
					doc = scripts[ scripts.length - 1 ].ownerDocument;

					// Reenable scripts
					jQuery.map( scripts, restoreScript );

					// Evaluate executable scripts on first document insertion
					for ( i = 0; i < hasScripts; i++ ) {
						node = scripts[ i ];
						if ( rscriptType.test( node.type || "" ) &&
							!data_priv.access( node, "globalEval" ) && jQuery.contains( doc, node ) ) {

							if ( node.src ) {
								// Optional AJAX dependency, but won't run scripts if not present
								if ( jQuery._evalUrl ) {
									jQuery._evalUrl( node.src );
								}
							} else {
								jQuery.globalEval( node.textContent.replace( rcleanScript, "" ) );
							}
						}
					}
				}
			}
		}

		return this;
	}
});

jQuery.each({
	appendTo: "append",
	prependTo: "prepend",
	insertBefore: "before",
	insertAfter: "after",
	replaceAll: "replaceWith"
}, function( name, original ) {
	jQuery.fn[ name ] = function( selector ) {
		var elems,
			ret = [],
			insert = jQuery( selector ),
			last = insert.length - 1,
			i = 0;

		for ( ; i <= last; i++ ) {
			elems = i === last ? this : this.clone( true );
			jQuery( insert[ i ] )[ original ]( elems );

			// Support: QtWebKit
			// .get() because push.apply(_, arraylike) throws
			push.apply( ret, elems.get() );
		}

		return this.pushStack( ret );
	};
});


var iframe,
	elemdisplay = {};

/**
 * Retrieve the actual display of a element
 * @param {String} name nodeName of the element
 * @param {Object} doc Document object
 */
// Called only from within defaultDisplay
function actualDisplay( name, doc ) {
	var style,
		elem = jQuery( doc.createElement( name ) ).appendTo( doc.body ),

		// getDefaultComputedStyle might be reliably used only on attached element
		display = window.getDefaultComputedStyle && ( style = window.getDefaultComputedStyle( elem[ 0 ] ) ) ?

			// Use of this method is a temporary fix (more like optimization) until something better comes along,
			// since it was removed from specification and supported only in FF
			style.display : jQuery.css( elem[ 0 ], "display" );

	// We don't have any data stored on the element,
	// so use "detach" method as fast way to get rid of the element
	elem.detach();

	return display;
}

/**
 * Try to determine the default display value of an element
 * @param {String} nodeName
 */
function defaultDisplay( nodeName ) {
	var doc = document,
		display = elemdisplay[ nodeName ];

	if ( !display ) {
		display = actualDisplay( nodeName, doc );

		// If the simple way fails, read from inside an iframe
		if ( display === "none" || !display ) {

			// Use the already-created iframe if possible
			iframe = (iframe || jQuery( "<iframe frameborder='0' width='0' height='0'/>" )).appendTo( doc.documentElement );

			// Always write a new HTML skeleton so Webkit and Firefox don't choke on reuse
			doc = iframe[ 0 ].contentDocument;

			// Support: IE
			doc.write();
			doc.close();

			display = actualDisplay( nodeName, doc );
			iframe.detach();
		}

		// Store the correct default display
		elemdisplay[ nodeName ] = display;
	}

	return display;
}
var rmargin = (/^margin/);

var rnumnonpx = new RegExp( "^(" + pnum + ")(?!px)[a-z%]+$", "i" );

var getStyles = function( elem ) {
		// Support: IE<=11+, Firefox<=30+ (#15098, #14150)
		// IE throws on elements created in popups
		// FF meanwhile throws on frame elements through "defaultView.getComputedStyle"
		if ( elem.ownerDocument.defaultView.opener ) {
			return elem.ownerDocument.defaultView.getComputedStyle( elem, null );
		}

		return window.getComputedStyle( elem, null );
	};



function curCSS( elem, name, computed ) {
	var width, minWidth, maxWidth, ret,
		style = elem.style;

	computed = computed || getStyles( elem );

	// Support: IE9
	// getPropertyValue is only needed for .css('filter') (#12537)
	if ( computed ) {
		ret = computed.getPropertyValue( name ) || computed[ name ];
	}

	if ( computed ) {

		if ( ret === "" && !jQuery.contains( elem.ownerDocument, elem ) ) {
			ret = jQuery.style( elem, name );
		}

		// Support: iOS < 6
		// A tribute to the "awesome hack by Dean Edwards"
		// iOS < 6 (at least) returns percentage for a larger set of values, but width seems to be reliably pixels
		// this is against the CSSOM draft spec: http://dev.w3.org/csswg/cssom/#resolved-values
		if ( rnumnonpx.test( ret ) && rmargin.test( name ) ) {

			// Remember the original values
			width = style.width;
			minWidth = style.minWidth;
			maxWidth = style.maxWidth;

			// Put in the new values to get a computed value out
			style.minWidth = style.maxWidth = style.width = ret;
			ret = computed.width;

			// Revert the changed values
			style.width = width;
			style.minWidth = minWidth;
			style.maxWidth = maxWidth;
		}
	}

	return ret !== undefined ?
		// Support: IE
		// IE returns zIndex value as an integer.
		ret + "" :
		ret;
}


function addGetHookIf( conditionFn, hookFn ) {
	// Define the hook, we'll check on the first run if it's really needed.
	return {
		get: function() {
			if ( conditionFn() ) {
				// Hook not needed (or it's not possible to use it due
				// to missing dependency), remove it.
				delete this.get;
				return;
			}

			// Hook needed; redefine it so that the support test is not executed again.
			return (this.get = hookFn).apply( this, arguments );
		}
	};
}


(function() {
	var pixelPositionVal, boxSizingReliableVal,
		docElem = document.documentElement,
		container = document.createElement( "div" ),
		div = document.createElement( "div" );

	if ( !div.style ) {
		return;
	}

	// Support: IE9-11+
	// Style of cloned element affects source element cloned (#8908)
	div.style.backgroundClip = "content-box";
	div.cloneNode( true ).style.backgroundClip = "";
	support.clearCloneStyle = div.style.backgroundClip === "content-box";

	container.style.cssText = "border:0;width:0;height:0;top:0;left:-9999px;margin-top:1px;" +
		"position:absolute";
	container.appendChild( div );

	// Executing both pixelPosition & boxSizingReliable tests require only one layout
	// so they're executed at the same time to save the second computation.
	function computePixelPositionAndBoxSizingReliable() {
		div.style.cssText =
			// Support: Firefox<29, Android 2.3
			// Vendor-prefix box-sizing
			"-webkit-box-sizing:border-box;-moz-box-sizing:border-box;" +
			"box-sizing:border-box;display:block;margin-top:1%;top:1%;" +
			"border:1px;padding:1px;width:4px;position:absolute";
		div.innerHTML = "";
		docElem.appendChild( container );

		var divStyle = window.getComputedStyle( div, null );
		pixelPositionVal = divStyle.top !== "1%";
		boxSizingReliableVal = divStyle.width === "4px";

		docElem.removeChild( container );
	}

	// Support: node.js jsdom
	// Don't assume that getComputedStyle is a property of the global object
	if ( window.getComputedStyle ) {
		jQuery.extend( support, {
			pixelPosition: function() {

				// This test is executed only once but we still do memoizing
				// since we can use the boxSizingReliable pre-computing.
				// No need to check if the test was already performed, though.
				computePixelPositionAndBoxSizingReliable();
				return pixelPositionVal;
			},
			boxSizingReliable: function() {
				if ( boxSizingReliableVal == null ) {
					computePixelPositionAndBoxSizingReliable();
				}
				return boxSizingReliableVal;
			},
			reliableMarginRight: function() {

				// Support: Android 2.3
				// Check if div with explicit width and no margin-right incorrectly
				// gets computed margin-right based on width of container. (#3333)
				// WebKit Bug 13343 - getComputedStyle returns wrong value for margin-right
				// This support function is only executed once so no memoizing is needed.
				var ret,
					marginDiv = div.appendChild( document.createElement( "div" ) );

				// Reset CSS: box-sizing; display; margin; border; padding
				marginDiv.style.cssText = div.style.cssText =
					// Support: Firefox<29, Android 2.3
					// Vendor-prefix box-sizing
					"-webkit-box-sizing:content-box;-moz-box-sizing:content-box;" +
					"box-sizing:content-box;display:block;margin:0;border:0;padding:0";
				marginDiv.style.marginRight = marginDiv.style.width = "0";
				div.style.width = "1px";
				docElem.appendChild( container );

				ret = !parseFloat( window.getComputedStyle( marginDiv, null ).marginRight );

				docElem.removeChild( container );
				div.removeChild( marginDiv );

				return ret;
			}
		});
	}
})();


// A method for quickly swapping in/out CSS properties to get correct calculations.
jQuery.swap = function( elem, options, callback, args ) {
	var ret, name,
		old = {};

	// Remember the old values, and insert the new ones
	for ( name in options ) {
		old[ name ] = elem.style[ name ];
		elem.style[ name ] = options[ name ];
	}

	ret = callback.apply( elem, args || [] );

	// Revert the old values
	for ( name in options ) {
		elem.style[ name ] = old[ name ];
	}

	return ret;
};


var
	// Swappable if display is none or starts with table except "table", "table-cell", or "table-caption"
	// See here for display values: https://developer.mozilla.org/en-US/docs/CSS/display
	rdisplayswap = /^(none|table(?!-c[ea]).+)/,
	rnumsplit = new RegExp( "^(" + pnum + ")(.*)$", "i" ),
	rrelNum = new RegExp( "^([+-])=(" + pnum + ")", "i" ),

	cssShow = { position: "absolute", visibility: "hidden", display: "block" },
	cssNormalTransform = {
		letterSpacing: "0",
		fontWeight: "400"
	},

	cssPrefixes = [ "Webkit", "O", "Moz", "ms" ];

// Return a css property mapped to a potentially vendor prefixed property
function vendorPropName( style, name ) {

	// Shortcut for names that are not vendor prefixed
	if ( name in style ) {
		return name;
	}

	// Check for vendor prefixed names
	var capName = name[0].toUpperCase() + name.slice(1),
		origName = name,
		i = cssPrefixes.length;

	while ( i-- ) {
		name = cssPrefixes[ i ] + capName;
		if ( name in style ) {
			return name;
		}
	}

	return origName;
}

function setPositiveNumber( elem, value, subtract ) {
	var matches = rnumsplit.exec( value );
	return matches ?
		// Guard against undefined "subtract", e.g., when used as in cssHooks
		Math.max( 0, matches[ 1 ] - ( subtract || 0 ) ) + ( matches[ 2 ] || "px" ) :
		value;
}

function augmentWidthOrHeight( elem, name, extra, isBorderBox, styles ) {
	var i = extra === ( isBorderBox ? "border" : "content" ) ?
		// If we already have the right measurement, avoid augmentation
		4 :
		// Otherwise initialize for horizontal or vertical properties
		name === "width" ? 1 : 0,

		val = 0;

	for ( ; i < 4; i += 2 ) {
		// Both box models exclude margin, so add it if we want it
		if ( extra === "margin" ) {
			val += jQuery.css( elem, extra + cssExpand[ i ], true, styles );
		}

		if ( isBorderBox ) {
			// border-box includes padding, so remove it if we want content
			if ( extra === "content" ) {
				val -= jQuery.css( elem, "padding" + cssExpand[ i ], true, styles );
			}

			// At this point, extra isn't border nor margin, so remove border
			if ( extra !== "margin" ) {
				val -= jQuery.css( elem, "border" + cssExpand[ i ] + "Width", true, styles );
			}
		} else {
			// At this point, extra isn't content, so add padding
			val += jQuery.css( elem, "padding" + cssExpand[ i ], true, styles );

			// At this point, extra isn't content nor padding, so add border
			if ( extra !== "padding" ) {
				val += jQuery.css( elem, "border" + cssExpand[ i ] + "Width", true, styles );
			}
		}
	}

	return val;
}

function getWidthOrHeight( elem, name, extra ) {

	// Start with offset property, which is equivalent to the border-box value
	var valueIsBorderBox = true,
		val = name === "width" ? elem.offsetWidth : elem.offsetHeight,
		styles = getStyles( elem ),
		isBorderBox = jQuery.css( elem, "boxSizing", false, styles ) === "border-box";

	// Some non-html elements return undefined for offsetWidth, so check for null/undefined
	// svg - https://bugzilla.mozilla.org/show_bug.cgi?id=649285
	// MathML - https://bugzilla.mozilla.org/show_bug.cgi?id=491668
	if ( val <= 0 || val == null ) {
		// Fall back to computed then uncomputed css if necessary
		val = curCSS( elem, name, styles );
		if ( val < 0 || val == null ) {
			val = elem.style[ name ];
		}

		// Computed unit is not pixels. Stop here and return.
		if ( rnumnonpx.test(val) ) {
			return val;
		}

		// Check for style in case a browser which returns unreliable values
		// for getComputedStyle silently falls back to the reliable elem.style
		valueIsBorderBox = isBorderBox &&
			( support.boxSizingReliable() || val === elem.style[ name ] );

		// Normalize "", auto, and prepare for extra
		val = parseFloat( val ) || 0;
	}

	// Use the active box-sizing model to add/subtract irrelevant styles
	return ( val +
		augmentWidthOrHeight(
			elem,
			name,
			extra || ( isBorderBox ? "border" : "content" ),
			valueIsBorderBox,
			styles
		)
	) + "px";
}

function showHide( elements, show ) {
	var display, elem, hidden,
		values = [],
		index = 0,
		length = elements.length;

	for ( ; index < length; index++ ) {
		elem = elements[ index ];
		if ( !elem.style ) {
			continue;
		}

		values[ index ] = data_priv.get( elem, "olddisplay" );
		display = elem.style.display;
		if ( show ) {
			// Reset the inline display of this element to learn if it is
			// being hidden by cascaded rules or not
			if ( !values[ index ] && display === "none" ) {
				elem.style.display = "";
			}

			// Set elements which have been overridden with display: none
			// in a stylesheet to whatever the default browser style is
			// for such an element
			if ( elem.style.display === "" && isHidden( elem ) ) {
				values[ index ] = data_priv.access( elem, "olddisplay", defaultDisplay(elem.nodeName) );
			}
		} else {
			hidden = isHidden( elem );

			if ( display !== "none" || !hidden ) {
				data_priv.set( elem, "olddisplay", hidden ? display : jQuery.css( elem, "display" ) );
			}
		}
	}

	// Set the display of most of the elements in a second loop
	// to avoid the constant reflow
	for ( index = 0; index < length; index++ ) {
		elem = elements[ index ];
		if ( !elem.style ) {
			continue;
		}
		if ( !show || elem.style.display === "none" || elem.style.display === "" ) {
			elem.style.display = show ? values[ index ] || "" : "none";
		}
	}

	return elements;
}

jQuery.extend({

	// Add in style property hooks for overriding the default
	// behavior of getting and setting a style property
	cssHooks: {
		opacity: {
			get: function( elem, computed ) {
				if ( computed ) {

					// We should always get a number back from opacity
					var ret = curCSS( elem, "opacity" );
					return ret === "" ? "1" : ret;
				}
			}
		}
	},

	// Don't automatically add "px" to these possibly-unitless properties
	cssNumber: {
		"columnCount": true,
		"fillOpacity": true,
		"flexGrow": true,
		"flexShrink": true,
		"fontWeight": true,
		"lineHeight": true,
		"opacity": true,
		"order": true,
		"orphans": true,
		"widows": true,
		"zIndex": true,
		"zoom": true
	},

	// Add in properties whose names you wish to fix before
	// setting or getting the value
	cssProps: {
		"float": "cssFloat"
	},

	// Get and set the style property on a DOM Node
	style: function( elem, name, value, extra ) {

		// Don't set styles on text and comment nodes
		if ( !elem || elem.nodeType === 3 || elem.nodeType === 8 || !elem.style ) {
			return;
		}

		// Make sure that we're working with the right name
		var ret, type, hooks,
			origName = jQuery.camelCase( name ),
			style = elem.style;

		name = jQuery.cssProps[ origName ] || ( jQuery.cssProps[ origName ] = vendorPropName( style, origName ) );

		// Gets hook for the prefixed version, then unprefixed version
		hooks = jQuery.cssHooks[ name ] || jQuery.cssHooks[ origName ];

		// Check if we're setting a value
		if ( value !== undefined ) {
			type = typeof value;

			// Convert "+=" or "-=" to relative numbers (#7345)
			if ( type === "string" && (ret = rrelNum.exec( value )) ) {
				value = ( ret[1] + 1 ) * ret[2] + parseFloat( jQuery.css( elem, name ) );
				// Fixes bug #9237
				type = "number";
			}

			// Make sure that null and NaN values aren't set (#7116)
			if ( value == null || value !== value ) {
				return;
			}

			// If a number, add 'px' to the (except for certain CSS properties)
			if ( type === "number" && !jQuery.cssNumber[ origName ] ) {
				value += "px";
			}

			// Support: IE9-11+
			// background-* props affect original clone's values
			if ( !support.clearCloneStyle && value === "" && name.indexOf( "background" ) === 0 ) {
				style[ name ] = "inherit";
			}

			// If a hook was provided, use that value, otherwise just set the specified value
			if ( !hooks || !("set" in hooks) || (value = hooks.set( elem, value, extra )) !== undefined ) {
				style[ name ] = value;
			}

		} else {
			// If a hook was provided get the non-computed value from there
			if ( hooks && "get" in hooks && (ret = hooks.get( elem, false, extra )) !== undefined ) {
				return ret;
			}

			// Otherwise just get the value from the style object
			return style[ name ];
		}
	},

	css: function( elem, name, extra, styles ) {
		var val, num, hooks,
			origName = jQuery.camelCase( name );

		// Make sure that we're working with the right name
		name = jQuery.cssProps[ origName ] || ( jQuery.cssProps[ origName ] = vendorPropName( elem.style, origName ) );

		// Try prefixed name followed by the unprefixed name
		hooks = jQuery.cssHooks[ name ] || jQuery.cssHooks[ origName ];

		// If a hook was provided get the computed value from there
		if ( hooks && "get" in hooks ) {
			val = hooks.get( elem, true, extra );
		}

		// Otherwise, if a way to get the computed value exists, use that
		if ( val === undefined ) {
			val = curCSS( elem, name, styles );
		}

		// Convert "normal" to computed value
		if ( val === "normal" && name in cssNormalTransform ) {
			val = cssNormalTransform[ name ];
		}

		// Make numeric if forced or a qualifier was provided and val looks numeric
		if ( extra === "" || extra ) {
			num = parseFloat( val );
			return extra === true || jQuery.isNumeric( num ) ? num || 0 : val;
		}
		return val;
	}
});

jQuery.each([ "height", "width" ], function( i, name ) {
	jQuery.cssHooks[ name ] = {
		get: function( elem, computed, extra ) {
			if ( computed ) {

				// Certain elements can have dimension info if we invisibly show them
				// but it must have a current display style that would benefit
				return rdisplayswap.test( jQuery.css( elem, "display" ) ) && elem.offsetWidth === 0 ?
					jQuery.swap( elem, cssShow, function() {
						return getWidthOrHeight( elem, name, extra );
					}) :
					getWidthOrHeight( elem, name, extra );
			}
		},

		set: function( elem, value, extra ) {
			var styles = extra && getStyles( elem );
			return setPositiveNumber( elem, value, extra ?
				augmentWidthOrHeight(
					elem,
					name,
					extra,
					jQuery.css( elem, "boxSizing", false, styles ) === "border-box",
					styles
				) : 0
			);
		}
	};
});

// Support: Android 2.3
jQuery.cssHooks.marginRight = addGetHookIf( support.reliableMarginRight,
	function( elem, computed ) {
		if ( computed ) {
			return jQuery.swap( elem, { "display": "inline-block" },
				curCSS, [ elem, "marginRight" ] );
		}
	}
);

// These hooks are used by animate to expand properties
jQuery.each({
	margin: "",
	padding: "",
	border: "Width"
}, function( prefix, suffix ) {
	jQuery.cssHooks[ prefix + suffix ] = {
		expand: function( value ) {
			var i = 0,
				expanded = {},

				// Assumes a single number if not a string
				parts = typeof value === "string" ? value.split(" ") : [ value ];

			for ( ; i < 4; i++ ) {
				expanded[ prefix + cssExpand[ i ] + suffix ] =
					parts[ i ] || parts[ i - 2 ] || parts[ 0 ];
			}

			return expanded;
		}
	};

	if ( !rmargin.test( prefix ) ) {
		jQuery.cssHooks[ prefix + suffix ].set = setPositiveNumber;
	}
});

jQuery.fn.extend({
	css: function( name, value ) {
		return access( this, function( elem, name, value ) {
			var styles, len,
				map = {},
				i = 0;

			if ( jQuery.isArray( name ) ) {
				styles = getStyles( elem );
				len = name.length;

				for ( ; i < len; i++ ) {
					map[ name[ i ] ] = jQuery.css( elem, name[ i ], false, styles );
				}

				return map;
			}

			return value !== undefined ?
				jQuery.style( elem, name, value ) :
				jQuery.css( elem, name );
		}, name, value, arguments.length > 1 );
	},
	show: function() {
		return showHide( this, true );
	},
	hide: function() {
		return showHide( this );
	},
	toggle: function( state ) {
		if ( typeof state === "boolean" ) {
			return state ? this.show() : this.hide();
		}

		return this.each(function() {
			if ( isHidden( this ) ) {
				jQuery( this ).show();
			} else {
				jQuery( this ).hide();
			}
		});
	}
});


function Tween( elem, options, prop, end, easing ) {
	return new Tween.prototype.init( elem, options, prop, end, easing );
}
jQuery.Tween = Tween;

Tween.prototype = {
	constructor: Tween,
	init: function( elem, options, prop, end, easing, unit ) {
		this.elem = elem;
		this.prop = prop;
		this.easing = easing || "swing";
		this.options = options;
		this.start = this.now = this.cur();
		this.end = end;
		this.unit = unit || ( jQuery.cssNumber[ prop ] ? "" : "px" );
	},
	cur: function() {
		var hooks = Tween.propHooks[ this.prop ];

		return hooks && hooks.get ?
			hooks.get( this ) :
			Tween.propHooks._default.get( this );
	},
	run: function( percent ) {
		var eased,
			hooks = Tween.propHooks[ this.prop ];

		if ( this.options.duration ) {
			this.pos = eased = jQuery.easing[ this.easing ](
				percent, this.options.duration * percent, 0, 1, this.options.duration
			);
		} else {
			this.pos = eased = percent;
		}
		this.now = ( this.end - this.start ) * eased + this.start;

		if ( this.options.step ) {
			this.options.step.call( this.elem, this.now, this );
		}

		if ( hooks && hooks.set ) {
			hooks.set( this );
		} else {
			Tween.propHooks._default.set( this );
		}
		return this;
	}
};

Tween.prototype.init.prototype = Tween.prototype;

Tween.propHooks = {
	_default: {
		get: function( tween ) {
			var result;

			if ( tween.elem[ tween.prop ] != null &&
				(!tween.elem.style || tween.elem.style[ tween.prop ] == null) ) {
				return tween.elem[ tween.prop ];
			}

			// Passing an empty string as a 3rd parameter to .css will automatically
			// attempt a parseFloat and fallback to a string if the parse fails.
			// Simple values such as "10px" are parsed to Float;
			// complex values such as "rotate(1rad)" are returned as-is.
			result = jQuery.css( tween.elem, tween.prop, "" );
			// Empty strings, null, undefined and "auto" are converted to 0.
			return !result || result === "auto" ? 0 : result;
		},
		set: function( tween ) {
			// Use step hook for back compat.
			// Use cssHook if its there.
			// Use .style if available and use plain properties where available.
			if ( jQuery.fx.step[ tween.prop ] ) {
				jQuery.fx.step[ tween.prop ]( tween );
			} else if ( tween.elem.style && ( tween.elem.style[ jQuery.cssProps[ tween.prop ] ] != null || jQuery.cssHooks[ tween.prop ] ) ) {
				jQuery.style( tween.elem, tween.prop, tween.now + tween.unit );
			} else {
				tween.elem[ tween.prop ] = tween.now;
			}
		}
	}
};

// Support: IE9
// Panic based approach to setting things on disconnected nodes
Tween.propHooks.scrollTop = Tween.propHooks.scrollLeft = {
	set: function( tween ) {
		if ( tween.elem.nodeType && tween.elem.parentNode ) {
			tween.elem[ tween.prop ] = tween.now;
		}
	}
};

jQuery.easing = {
	linear: function( p ) {
		return p;
	},
	swing: function( p ) {
		return 0.5 - Math.cos( p * Math.PI ) / 2;
	}
};

jQuery.fx = Tween.prototype.init;

// Back Compat <1.8 extension point
jQuery.fx.step = {};




var
	fxNow, timerId,
	rfxtypes = /^(?:toggle|show|hide)$/,
	rfxnum = new RegExp( "^(?:([+-])=|)(" + pnum + ")([a-z%]*)$", "i" ),
	rrun = /queueHooks$/,
	animationPrefilters = [ defaultPrefilter ],
	tweeners = {
		"*": [ function( prop, value ) {
			var tween = this.createTween( prop, value ),
				target = tween.cur(),
				parts = rfxnum.exec( value ),
				unit = parts && parts[ 3 ] || ( jQuery.cssNumber[ prop ] ? "" : "px" ),

				// Starting value computation is required for potential unit mismatches
				start = ( jQuery.cssNumber[ prop ] || unit !== "px" && +target ) &&
					rfxnum.exec( jQuery.css( tween.elem, prop ) ),
				scale = 1,
				maxIterations = 20;

			if ( start && start[ 3 ] !== unit ) {
				// Trust units reported by jQuery.css
				unit = unit || start[ 3 ];

				// Make sure we update the tween properties later on
				parts = parts || [];

				// Iteratively approximate from a nonzero starting point
				start = +target || 1;

				do {
					// If previous iteration zeroed out, double until we get *something*.
					// Use string for doubling so we don't accidentally see scale as unchanged below
					scale = scale || ".5";

					// Adjust and apply
					start = start / scale;
					jQuery.style( tween.elem, prop, start + unit );

				// Update scale, tolerating zero or NaN from tween.cur(),
				// break the loop if scale is unchanged or perfect, or if we've just had enough
				} while ( scale !== (scale = tween.cur() / target) && scale !== 1 && --maxIterations );
			}

			// Update tween properties
			if ( parts ) {
				start = tween.start = +start || +target || 0;
				tween.unit = unit;
				// If a +=/-= token was provided, we're doing a relative animation
				tween.end = parts[ 1 ] ?
					start + ( parts[ 1 ] + 1 ) * parts[ 2 ] :
					+parts[ 2 ];
			}

			return tween;
		} ]
	};

// Animations created synchronously will run synchronously
function createFxNow() {
	setTimeout(function() {
		fxNow = undefined;
	});
	return ( fxNow = jQuery.now() );
}

// Generate parameters to create a standard animation
function genFx( type, includeWidth ) {
	var which,
		i = 0,
		attrs = { height: type };

	// If we include width, step value is 1 to do all cssExpand values,
	// otherwise step value is 2 to skip over Left and Right
	includeWidth = includeWidth ? 1 : 0;
	for ( ; i < 4 ; i += 2 - includeWidth ) {
		which = cssExpand[ i ];
		attrs[ "margin" + which ] = attrs[ "padding" + which ] = type;
	}

	if ( includeWidth ) {
		attrs.opacity = attrs.width = type;
	}

	return attrs;
}

function createTween( value, prop, animation ) {
	var tween,
		collection = ( tweeners[ prop ] || [] ).concat( tweeners[ "*" ] ),
		index = 0,
		length = collection.length;
	for ( ; index < length; index++ ) {
		if ( (tween = collection[ index ].call( animation, prop, value )) ) {

			// We're done with this property
			return tween;
		}
	}
}

function defaultPrefilter( elem, props, opts ) {
	/* jshint validthis: true */
	var prop, value, toggle, tween, hooks, oldfire, display, checkDisplay,
		anim = this,
		orig = {},
		style = elem.style,
		hidden = elem.nodeType && isHidden( elem ),
		dataShow = data_priv.get( elem, "fxshow" );

	// Handle queue: false promises
	if ( !opts.queue ) {
		hooks = jQuery._queueHooks( elem, "fx" );
		if ( hooks.unqueued == null ) {
			hooks.unqueued = 0;
			oldfire = hooks.empty.fire;
			hooks.empty.fire = function() {
				if ( !hooks.unqueued ) {
					oldfire();
				}
			};
		}
		hooks.unqueued++;

		anim.always(function() {
			// Ensure the complete handler is called before this completes
			anim.always(function() {
				hooks.unqueued--;
				if ( !jQuery.queue( elem, "fx" ).length ) {
					hooks.empty.fire();
				}
			});
		});
	}

	// Height/width overflow pass
	if ( elem.nodeType === 1 && ( "height" in props || "width" in props ) ) {
		// Make sure that nothing sneaks out
		// Record all 3 overflow attributes because IE9-10 do not
		// change the overflow attribute when overflowX and
		// overflowY are set to the same value
		opts.overflow = [ style.overflow, style.overflowX, style.overflowY ];

		// Set display property to inline-block for height/width
		// animations on inline elements that are having width/height animated
		display = jQuery.css( elem, "display" );

		// Test default display if display is currently "none"
		checkDisplay = display === "none" ?
			data_priv.get( elem, "olddisplay" ) || defaultDisplay( elem.nodeName ) : display;

		if ( checkDisplay === "inline" && jQuery.css( elem, "float" ) === "none" ) {
			style.display = "inline-block";
		}
	}

	if ( opts.overflow ) {
		style.overflow = "hidden";
		anim.always(function() {
			style.overflow = opts.overflow[ 0 ];
			style.overflowX = opts.overflow[ 1 ];
			style.overflowY = opts.overflow[ 2 ];
		});
	}

	// show/hide pass
	for ( prop in props ) {
		value = props[ prop ];
		if ( rfxtypes.exec( value ) ) {
			delete props[ prop ];
			toggle = toggle || value === "toggle";
			if ( value === ( hidden ? "hide" : "show" ) ) {

				// If there is dataShow left over from a stopped hide or show and we are going to proceed with show, we should pretend to be hidden
				if ( value === "show" && dataShow && dataShow[ prop ] !== undefined ) {
					hidden = true;
				} else {
					continue;
				}
			}
			orig[ prop ] = dataShow && dataShow[ prop ] || jQuery.style( elem, prop );

		// Any non-fx value stops us from restoring the original display value
		} else {
			display = undefined;
		}
	}

	if ( !jQuery.isEmptyObject( orig ) ) {
		if ( dataShow ) {
			if ( "hidden" in dataShow ) {
				hidden = dataShow.hidden;
			}
		} else {
			dataShow = data_priv.access( elem, "fxshow", {} );
		}

		// Store state if its toggle - enables .stop().toggle() to "reverse"
		if ( toggle ) {
			dataShow.hidden = !hidden;
		}
		if ( hidden ) {
			jQuery( elem ).show();
		} else {
			anim.done(function() {
				jQuery( elem ).hide();
			});
		}
		anim.done(function() {
			var prop;

			data_priv.remove( elem, "fxshow" );
			for ( prop in orig ) {
				jQuery.style( elem, prop, orig[ prop ] );
			}
		});
		for ( prop in orig ) {
			tween = createTween( hidden ? dataShow[ prop ] : 0, prop, anim );

			if ( !( prop in dataShow ) ) {
				dataShow[ prop ] = tween.start;
				if ( hidden ) {
					tween.end = tween.start;
					tween.start = prop === "width" || prop === "height" ? 1 : 0;
				}
			}
		}

	// If this is a noop like .hide().hide(), restore an overwritten display value
	} else if ( (display === "none" ? defaultDisplay( elem.nodeName ) : display) === "inline" ) {
		style.display = display;
	}
}

function propFilter( props, specialEasing ) {
	var index, name, easing, value, hooks;

	// camelCase, specialEasing and expand cssHook pass
	for ( index in props ) {
		name = jQuery.camelCase( index );
		easing = specialEasing[ name ];
		value = props[ index ];
		if ( jQuery.isArray( value ) ) {
			easing = value[ 1 ];
			value = props[ index ] = value[ 0 ];
		}

		if ( index !== name ) {
			props[ name ] = value;
			delete props[ index ];
		}

		hooks = jQuery.cssHooks[ name ];
		if ( hooks && "expand" in hooks ) {
			value = hooks.expand( value );
			delete props[ name ];

			// Not quite $.extend, this won't overwrite existing keys.
			// Reusing 'index' because we have the correct "name"
			for ( index in value ) {
				if ( !( index in props ) ) {
					props[ index ] = value[ index ];
					specialEasing[ index ] = easing;
				}
			}
		} else {
			specialEasing[ name ] = easing;
		}
	}
}

function Animation( elem, properties, options ) {
	var result,
		stopped,
		index = 0,
		length = animationPrefilters.length,
		deferred = jQuery.Deferred().always( function() {
			// Don't match elem in the :animated selector
			delete tick.elem;
		}),
		tick = function() {
			if ( stopped ) {
				return false;
			}
			var currentTime = fxNow || createFxNow(),
				remaining = Math.max( 0, animation.startTime + animation.duration - currentTime ),
				// Support: Android 2.3
				// Archaic crash bug won't allow us to use `1 - ( 0.5 || 0 )` (#12497)
				temp = remaining / animation.duration || 0,
				percent = 1 - temp,
				index = 0,
				length = animation.tweens.length;

			for ( ; index < length ; index++ ) {
				animation.tweens[ index ].run( percent );
			}

			deferred.notifyWith( elem, [ animation, percent, remaining ]);

			if ( percent < 1 && length ) {
				return remaining;
			} else {
				deferred.resolveWith( elem, [ animation ] );
				return false;
			}
		},
		animation = deferred.promise({
			elem: elem,
			props: jQuery.extend( {}, properties ),
			opts: jQuery.extend( true, { specialEasing: {} }, options ),
			originalProperties: properties,
			originalOptions: options,
			startTime: fxNow || createFxNow(),
			duration: options.duration,
			tweens: [],
			createTween: function( prop, end ) {
				var tween = jQuery.Tween( elem, animation.opts, prop, end,
						animation.opts.specialEasing[ prop ] || animation.opts.easing );
				animation.tweens.push( tween );
				return tween;
			},
			stop: function( gotoEnd ) {
				var index = 0,
					// If we are going to the end, we want to run all the tweens
					// otherwise we skip this part
					length = gotoEnd ? animation.tweens.length : 0;
				if ( stopped ) {
					return this;
				}
				stopped = true;
				for ( ; index < length ; index++ ) {
					animation.tweens[ index ].run( 1 );
				}

				// Resolve when we played the last frame; otherwise, reject
				if ( gotoEnd ) {
					deferred.resolveWith( elem, [ animation, gotoEnd ] );
				} else {
					deferred.rejectWith( elem, [ animation, gotoEnd ] );
				}
				return this;
			}
		}),
		props = animation.props;

	propFilter( props, animation.opts.specialEasing );

	for ( ; index < length ; index++ ) {
		result = animationPrefilters[ index ].call( animation, elem, props, animation.opts );
		if ( result ) {
			return result;
		}
	}

	jQuery.map( props, createTween, animation );

	if ( jQuery.isFunction( animation.opts.start ) ) {
		animation.opts.start.call( elem, animation );
	}

	jQuery.fx.timer(
		jQuery.extend( tick, {
			elem: elem,
			anim: animation,
			queue: animation.opts.queue
		})
	);

	// attach callbacks from options
	return animation.progress( animation.opts.progress )
		.done( animation.opts.done, animation.opts.complete )
		.fail( animation.opts.fail )
		.always( animation.opts.always );
}

jQuery.Animation = jQuery.extend( Animation, {

	tweener: function( props, callback ) {
		if ( jQuery.isFunction( props ) ) {
			callback = props;
			props = [ "*" ];
		} else {
			props = props.split(" ");
		}

		var prop,
			index = 0,
			length = props.length;

		for ( ; index < length ; index++ ) {
			prop = props[ index ];
			tweeners[ prop ] = tweeners[ prop ] || [];
			tweeners[ prop ].unshift( callback );
		}
	},

	prefilter: function( callback, prepend ) {
		if ( prepend ) {
			animationPrefilters.unshift( callback );
		} else {
			animationPrefilters.push( callback );
		}
	}
});

jQuery.speed = function( speed, easing, fn ) {
	var opt = speed && typeof speed === "object" ? jQuery.extend( {}, speed ) : {
		complete: fn || !fn && easing ||
			jQuery.isFunction( speed ) && speed,
		duration: speed,
		easing: fn && easing || easing && !jQuery.isFunction( easing ) && easing
	};

	opt.duration = jQuery.fx.off ? 0 : typeof opt.duration === "number" ? opt.duration :
		opt.duration in jQuery.fx.speeds ? jQuery.fx.speeds[ opt.duration ] : jQuery.fx.speeds._default;

	// Normalize opt.queue - true/undefined/null -> "fx"
	if ( opt.queue == null || opt.queue === true ) {
		opt.queue = "fx";
	}

	// Queueing
	opt.old = opt.complete;

	opt.complete = function() {
		if ( jQuery.isFunction( opt.old ) ) {
			opt.old.call( this );
		}

		if ( opt.queue ) {
			jQuery.dequeue( this, opt.queue );
		}
	};

	return opt;
};

jQuery.fn.extend({
	fadeTo: function( speed, to, easing, callback ) {

		// Show any hidden elements after setting opacity to 0
		return this.filter( isHidden ).css( "opacity", 0 ).show()

			// Animate to the value specified
			.end().animate({ opacity: to }, speed, easing, callback );
	},
	animate: function( prop, speed, easing, callback ) {
		var empty = jQuery.isEmptyObject( prop ),
			optall = jQuery.speed( speed, easing, callback ),
			doAnimation = function() {
				// Operate on a copy of prop so per-property easing won't be lost
				var anim = Animation( this, jQuery.extend( {}, prop ), optall );

				// Empty animations, or finishing resolves immediately
				if ( empty || data_priv.get( this, "finish" ) ) {
					anim.stop( true );
				}
			};
			doAnimation.finish = doAnimation;

		return empty || optall.queue === false ?
			this.each( doAnimation ) :
			this.queue( optall.queue, doAnimation );
	},
	stop: function( type, clearQueue, gotoEnd ) {
		var stopQueue = function( hooks ) {
			var stop = hooks.stop;
			delete hooks.stop;
			stop( gotoEnd );
		};

		if ( typeof type !== "string" ) {
			gotoEnd = clearQueue;
			clearQueue = type;
			type = undefined;
		}
		if ( clearQueue && type !== false ) {
			this.queue( type || "fx", [] );
		}

		return this.each(function() {
			var dequeue = true,
				index = type != null && type + "queueHooks",
				timers = jQuery.timers,
				data = data_priv.get( this );

			if ( index ) {
				if ( data[ index ] && data[ index ].stop ) {
					stopQueue( data[ index ] );
				}
			} else {
				for ( index in data ) {
					if ( data[ index ] && data[ index ].stop && rrun.test( index ) ) {
						stopQueue( data[ index ] );
					}
				}
			}

			for ( index = timers.length; index--; ) {
				if ( timers[ index ].elem === this && (type == null || timers[ index ].queue === type) ) {
					timers[ index ].anim.stop( gotoEnd );
					dequeue = false;
					timers.splice( index, 1 );
				}
			}

			// Start the next in the queue if the last step wasn't forced.
			// Timers currently will call their complete callbacks, which
			// will dequeue but only if they were gotoEnd.
			if ( dequeue || !gotoEnd ) {
				jQuery.dequeue( this, type );
			}
		});
	},
	finish: function( type ) {
		if ( type !== false ) {
			type = type || "fx";
		}
		return this.each(function() {
			var index,
				data = data_priv.get( this ),
				queue = data[ type + "queue" ],
				hooks = data[ type + "queueHooks" ],
				timers = jQuery.timers,
				length = queue ? queue.length : 0;

			// Enable finishing flag on private data
			data.finish = true;

			// Empty the queue first
			jQuery.queue( this, type, [] );

			if ( hooks && hooks.stop ) {
				hooks.stop.call( this, true );
			}

			// Look for any active animations, and finish them
			for ( index = timers.length; index--; ) {
				if ( timers[ index ].elem === this && timers[ index ].queue === type ) {
					timers[ index ].anim.stop( true );
					timers.splice( index, 1 );
				}
			}

			// Look for any animations in the old queue and finish them
			for ( index = 0; index < length; index++ ) {
				if ( queue[ index ] && queue[ index ].finish ) {
					queue[ index ].finish.call( this );
				}
			}

			// Turn off finishing flag
			delete data.finish;
		});
	}
});

jQuery.each([ "toggle", "show", "hide" ], function( i, name ) {
	var cssFn = jQuery.fn[ name ];
	jQuery.fn[ name ] = function( speed, easing, callback ) {
		return speed == null || typeof speed === "boolean" ?
			cssFn.apply( this, arguments ) :
			this.animate( genFx( name, true ), speed, easing, callback );
	};
});

// Generate shortcuts for custom animations
jQuery.each({
	slideDown: genFx("show"),
	slideUp: genFx("hide"),
	slideToggle: genFx("toggle"),
	fadeIn: { opacity: "show" },
	fadeOut: { opacity: "hide" },
	fadeToggle: { opacity: "toggle" }
}, function( name, props ) {
	jQuery.fn[ name ] = function( speed, easing, callback ) {
		return this.animate( props, speed, easing, callback );
	};
});

jQuery.timers = [];
jQuery.fx.tick = function() {
	var timer,
		i = 0,
		timers = jQuery.timers;

	fxNow = jQuery.now();

	for ( ; i < timers.length; i++ ) {
		timer = timers[ i ];
		// Checks the timer has not already been removed
		if ( !timer() && timers[ i ] === timer ) {
			timers.splice( i--, 1 );
		}
	}

	if ( !timers.length ) {
		jQuery.fx.stop();
	}
	fxNow = undefined;
};

jQuery.fx.timer = function( timer ) {
	jQuery.timers.push( timer );
	if ( timer() ) {
		jQuery.fx.start();
	} else {
		jQuery.timers.pop();
	}
};

jQuery.fx.interval = 13;

jQuery.fx.start = function() {
	if ( !timerId ) {
		timerId = setInterval( jQuery.fx.tick, jQuery.fx.interval );
	}
};

jQuery.fx.stop = function() {
	clearInterval( timerId );
	timerId = null;
};

jQuery.fx.speeds = {
	slow: 600,
	fast: 200,
	// Default speed
	_default: 400
};


// Based off of the plugin by Clint Helfers, with permission.
// http://blindsignals.com/index.php/2009/07/jquery-delay/
jQuery.fn.delay = function( time, type ) {
	time = jQuery.fx ? jQuery.fx.speeds[ time ] || time : time;
	type = type || "fx";

	return this.queue( type, function( next, hooks ) {
		var timeout = setTimeout( next, time );
		hooks.stop = function() {
			clearTimeout( timeout );
		};
	});
};


(function() {
	var input = document.createElement( "input" ),
		select = document.createElement( "select" ),
		opt = select.appendChild( document.createElement( "option" ) );

	input.type = "checkbox";

	// Support: iOS<=5.1, Android<=4.2+
	// Default value for a checkbox should be "on"
	support.checkOn = input.value !== "";

	// Support: IE<=11+
	// Must access selectedIndex to make default options select
	support.optSelected = opt.selected;

	// Support: Android<=2.3
	// Options inside disabled selects are incorrectly marked as disabled
	select.disabled = true;
	support.optDisabled = !opt.disabled;

	// Support: IE<=11+
	// An input loses its value after becoming a radio
	input = document.createElement( "input" );
	input.value = "t";
	input.type = "radio";
	support.radioValue = input.value === "t";
})();


var nodeHook, boolHook,
	attrHandle = jQuery.expr.attrHandle;

jQuery.fn.extend({
	attr: function( name, value ) {
		return access( this, jQuery.attr, name, value, arguments.length > 1 );
	},

	removeAttr: function( name ) {
		return this.each(function() {
			jQuery.removeAttr( this, name );
		});
	}
});

jQuery.extend({
	attr: function( elem, name, value ) {
		var hooks, ret,
			nType = elem.nodeType;

		// don't get/set attributes on text, comment and attribute nodes
		if ( !elem || nType === 3 || nType === 8 || nType === 2 ) {
			return;
		}

		// Fallback to prop when attributes are not supported
		if ( typeof elem.getAttribute === strundefined ) {
			return jQuery.prop( elem, name, value );
		}

		// All attributes are lowercase
		// Grab necessary hook if one is defined
		if ( nType !== 1 || !jQuery.isXMLDoc( elem ) ) {
			name = name.toLowerCase();
			hooks = jQuery.attrHooks[ name ] ||
				( jQuery.expr.match.bool.test( name ) ? boolHook : nodeHook );
		}

		if ( value !== undefined ) {

			if ( value === null ) {
				jQuery.removeAttr( elem, name );

			} else if ( hooks && "set" in hooks && (ret = hooks.set( elem, value, name )) !== undefined ) {
				return ret;

			} else {
				elem.setAttribute( name, value + "" );
				return value;
			}

		} else if ( hooks && "get" in hooks && (ret = hooks.get( elem, name )) !== null ) {
			return ret;

		} else {
			ret = jQuery.find.attr( elem, name );

			// Non-existent attributes return null, we normalize to undefined
			return ret == null ?
				undefined :
				ret;
		}
	},

	removeAttr: function( elem, value ) {
		var name, propName,
			i = 0,
			attrNames = value && value.match( rnotwhite );

		if ( attrNames && elem.nodeType === 1 ) {
			while ( (name = attrNames[i++]) ) {
				propName = jQuery.propFix[ name ] || name;

				// Boolean attributes get special treatment (#10870)
				if ( jQuery.expr.match.bool.test( name ) ) {
					// Set corresponding property to false
					elem[ propName ] = false;
				}

				elem.removeAttribute( name );
			}
		}
	},

	attrHooks: {
		type: {
			set: function( elem, value ) {
				if ( !support.radioValue && value === "radio" &&
					jQuery.nodeName( elem, "input" ) ) {
					var val = elem.value;
					elem.setAttribute( "type", value );
					if ( val ) {
						elem.value = val;
					}
					return value;
				}
			}
		}
	}
});

// Hooks for boolean attributes
boolHook = {
	set: function( elem, value, name ) {
		if ( value === false ) {
			// Remove boolean attributes when set to false
			jQuery.removeAttr( elem, name );
		} else {
			elem.setAttribute( name, name );
		}
		return name;
	}
};
jQuery.each( jQuery.expr.match.bool.source.match( /\w+/g ), function( i, name ) {
	var getter = attrHandle[ name ] || jQuery.find.attr;

	attrHandle[ name ] = function( elem, name, isXML ) {
		var ret, handle;
		if ( !isXML ) {
			// Avoid an infinite loop by temporarily removing this function from the getter
			handle = attrHandle[ name ];
			attrHandle[ name ] = ret;
			ret = getter( elem, name, isXML ) != null ?
				name.toLowerCase() :
				null;
			attrHandle[ name ] = handle;
		}
		return ret;
	};
});




var rfocusable = /^(?:input|select|textarea|button)$/i;

jQuery.fn.extend({
	prop: function( name, value ) {
		return access( this, jQuery.prop, name, value, arguments.length > 1 );
	},

	removeProp: function( name ) {
		return this.each(function() {
			delete this[ jQuery.propFix[ name ] || name ];
		});
	}
});

jQuery.extend({
	propFix: {
		"for": "htmlFor",
		"class": "className"
	},

	prop: function( elem, name, value ) {
		var ret, hooks, notxml,
			nType = elem.nodeType;

		// Don't get/set properties on text, comment and attribute nodes
		if ( !elem || nType === 3 || nType === 8 || nType === 2 ) {
			return;
		}

		notxml = nType !== 1 || !jQuery.isXMLDoc( elem );

		if ( notxml ) {
			// Fix name and attach hooks
			name = jQuery.propFix[ name ] || name;
			hooks = jQuery.propHooks[ name ];
		}

		if ( value !== undefined ) {
			return hooks && "set" in hooks && (ret = hooks.set( elem, value, name )) !== undefined ?
				ret :
				( elem[ name ] = value );

		} else {
			return hooks && "get" in hooks && (ret = hooks.get( elem, name )) !== null ?
				ret :
				elem[ name ];
		}
	},

	propHooks: {
		tabIndex: {
			get: function( elem ) {
				return elem.hasAttribute( "tabindex" ) || rfocusable.test( elem.nodeName ) || elem.href ?
					elem.tabIndex :
					-1;
			}
		}
	}
});

if ( !support.optSelected ) {
	jQuery.propHooks.selected = {
		get: function( elem ) {
			var parent = elem.parentNode;
			if ( parent && parent.parentNode ) {
				parent.parentNode.selectedIndex;
			}
			return null;
		}
	};
}

jQuery.each([
	"tabIndex",
	"readOnly",
	"maxLength",
	"cellSpacing",
	"cellPadding",
	"rowSpan",
	"colSpan",
	"useMap",
	"frameBorder",
	"contentEditable"
], function() {
	jQuery.propFix[ this.toLowerCase() ] = this;
});




var rclass = /[\t\r\n\f]/g;

jQuery.fn.extend({
	addClass: function( value ) {
		var classes, elem, cur, clazz, j, finalValue,
			proceed = typeof value === "string" && value,
			i = 0,
			len = this.length;

		if ( jQuery.isFunction( value ) ) {
			return this.each(function( j ) {
				jQuery( this ).addClass( value.call( this, j, this.className ) );
			});
		}

		if ( proceed ) {
			// The disjunction here is for better compressibility (see removeClass)
			classes = ( value || "" ).match( rnotwhite ) || [];

			for ( ; i < len; i++ ) {
				elem = this[ i ];
				cur = elem.nodeType === 1 && ( elem.className ?
					( " " + elem.className + " " ).replace( rclass, " " ) :
					" "
				);

				if ( cur ) {
					j = 0;
					while ( (clazz = classes[j++]) ) {
						if ( cur.indexOf( " " + clazz + " " ) < 0 ) {
							cur += clazz + " ";
						}
					}

					// only assign if different to avoid unneeded rendering.
					finalValue = jQuery.trim( cur );
					if ( elem.className !== finalValue ) {
						elem.className = finalValue;
					}
				}
			}
		}

		return this;
	},

	removeClass: function( value ) {
		var classes, elem, cur, clazz, j, finalValue,
			proceed = arguments.length === 0 || typeof value === "string" && value,
			i = 0,
			len = this.length;

		if ( jQuery.isFunction( value ) ) {
			return this.each(function( j ) {
				jQuery( this ).removeClass( value.call( this, j, this.className ) );
			});
		}
		if ( proceed ) {
			classes = ( value || "" ).match( rnotwhite ) || [];

			for ( ; i < len; i++ ) {
				elem = this[ i ];
				// This expression is here for better compressibility (see addClass)
				cur = elem.nodeType === 1 && ( elem.className ?
					( " " + elem.className + " " ).replace( rclass, " " ) :
					""
				);

				if ( cur ) {
					j = 0;
					while ( (clazz = classes[j++]) ) {
						// Remove *all* instances
						while ( cur.indexOf( " " + clazz + " " ) >= 0 ) {
							cur = cur.replace( " " + clazz + " ", " " );
						}
					}

					// Only assign if different to avoid unneeded rendering.
					finalValue = value ? jQuery.trim( cur ) : "";
					if ( elem.className !== finalValue ) {
						elem.className = finalValue;
					}
				}
			}
		}

		return this;
	},

	toggleClass: function( value, stateVal ) {
		var type = typeof value;

		if ( typeof stateVal === "boolean" && type === "string" ) {
			return stateVal ? this.addClass( value ) : this.removeClass( value );
		}

		if ( jQuery.isFunction( value ) ) {
			return this.each(function( i ) {
				jQuery( this ).toggleClass( value.call(this, i, this.className, stateVal), stateVal );
			});
		}

		return this.each(function() {
			if ( type === "string" ) {
				// Toggle individual class names
				var className,
					i = 0,
					self = jQuery( this ),
					classNames = value.match( rnotwhite ) || [];

				while ( (className = classNames[ i++ ]) ) {
					// Check each className given, space separated list
					if ( self.hasClass( className ) ) {
						self.removeClass( className );
					} else {
						self.addClass( className );
					}
				}

			// Toggle whole class name
			} else if ( type === strundefined || type === "boolean" ) {
				if ( this.className ) {
					// store className if set
					data_priv.set( this, "__className__", this.className );
				}

				// If the element has a class name or if we're passed `false`,
				// then remove the whole classname (if there was one, the above saved it).
				// Otherwise bring back whatever was previously saved (if anything),
				// falling back to the empty string if nothing was stored.
				this.className = this.className || value === false ? "" : data_priv.get( this, "__className__" ) || "";
			}
		});
	},

	hasClass: function( selector ) {
		var className = " " + selector + " ",
			i = 0,
			l = this.length;
		for ( ; i < l; i++ ) {
			if ( this[i].nodeType === 1 && (" " + this[i].className + " ").replace(rclass, " ").indexOf( className ) >= 0 ) {
				return true;
			}
		}

		return false;
	}
});




var rreturn = /\r/g;

jQuery.fn.extend({
	val: function( value ) {
		var hooks, ret, isFunction,
			elem = this[0];

		if ( !arguments.length ) {
			if ( elem ) {
				hooks = jQuery.valHooks[ elem.type ] || jQuery.valHooks[ elem.nodeName.toLowerCase() ];

				if ( hooks && "get" in hooks && (ret = hooks.get( elem, "value" )) !== undefined ) {
					return ret;
				}

				ret = elem.value;

				return typeof ret === "string" ?
					// Handle most common string cases
					ret.replace(rreturn, "") :
					// Handle cases where value is null/undef or number
					ret == null ? "" : ret;
			}

			return;
		}

		isFunction = jQuery.isFunction( value );

		return this.each(function( i ) {
			var val;

			if ( this.nodeType !== 1 ) {
				return;
			}

			if ( isFunction ) {
				val = value.call( this, i, jQuery( this ).val() );
			} else {
				val = value;
			}

			// Treat null/undefined as ""; convert numbers to string
			if ( val == null ) {
				val = "";

			} else if ( typeof val === "number" ) {
				val += "";

			} else if ( jQuery.isArray( val ) ) {
				val = jQuery.map( val, function( value ) {
					return value == null ? "" : value + "";
				});
			}

			hooks = jQuery.valHooks[ this.type ] || jQuery.valHooks[ this.nodeName.toLowerCase() ];

			// If set returns undefined, fall back to normal setting
			if ( !hooks || !("set" in hooks) || hooks.set( this, val, "value" ) === undefined ) {
				this.value = val;
			}
		});
	}
});

jQuery.extend({
	valHooks: {
		option: {
			get: function( elem ) {
				var val = jQuery.find.attr( elem, "value" );
				return val != null ?
					val :
					// Support: IE10-11+
					// option.text throws exceptions (#14686, #14858)
					jQuery.trim( jQuery.text( elem ) );
			}
		},
		select: {
			get: function( elem ) {
				var value, option,
					options = elem.options,
					index = elem.selectedIndex,
					one = elem.type === "select-one" || index < 0,
					values = one ? null : [],
					max = one ? index + 1 : options.length,
					i = index < 0 ?
						max :
						one ? index : 0;

				// Loop through all the selected options
				for ( ; i < max; i++ ) {
					option = options[ i ];

					// IE6-9 doesn't update selected after form reset (#2551)
					if ( ( option.selected || i === index ) &&
							// Don't return options that are disabled or in a disabled optgroup
							( support.optDisabled ? !option.disabled : option.getAttribute( "disabled" ) === null ) &&
							( !option.parentNode.disabled || !jQuery.nodeName( option.parentNode, "optgroup" ) ) ) {

						// Get the specific value for the option
						value = jQuery( option ).val();

						// We don't need an array for one selects
						if ( one ) {
							return value;
						}

						// Multi-Selects return an array
						values.push( value );
					}
				}

				return values;
			},

			set: function( elem, value ) {
				var optionSet, option,
					options = elem.options,
					values = jQuery.makeArray( value ),
					i = options.length;

				while ( i-- ) {
					option = options[ i ];
					if ( (option.selected = jQuery.inArray( option.value, values ) >= 0) ) {
						optionSet = true;
					}
				}

				// Force browsers to behave consistently when non-matching value is set
				if ( !optionSet ) {
					elem.selectedIndex = -1;
				}
				return values;
			}
		}
	}
});

// Radios and checkboxes getter/setter
jQuery.each([ "radio", "checkbox" ], function() {
	jQuery.valHooks[ this ] = {
		set: function( elem, value ) {
			if ( jQuery.isArray( value ) ) {
				return ( elem.checked = jQuery.inArray( jQuery(elem).val(), value ) >= 0 );
			}
		}
	};
	if ( !support.checkOn ) {
		jQuery.valHooks[ this ].get = function( elem ) {
			return elem.getAttribute("value") === null ? "on" : elem.value;
		};
	}
});




// Return jQuery for attributes-only inclusion


jQuery.each( ("blur focus focusin focusout load resize scroll unload click dblclick " +
	"mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave " +
	"change select submit keydown keypress keyup error contextmenu").split(" "), function( i, name ) {

	// Handle event binding
	jQuery.fn[ name ] = function( data, fn ) {
		return arguments.length > 0 ?
			this.on( name, null, data, fn ) :
			this.trigger( name );
	};
});

jQuery.fn.extend({
	hover: function( fnOver, fnOut ) {
		return this.mouseenter( fnOver ).mouseleave( fnOut || fnOver );
	},

	bind: function( types, data, fn ) {
		return this.on( types, null, data, fn );
	},
	unbind: function( types, fn ) {
		return this.off( types, null, fn );
	},

	delegate: function( selector, types, data, fn ) {
		return this.on( types, selector, data, fn );
	},
	undelegate: function( selector, types, fn ) {
		// ( namespace ) or ( selector, types [, fn] )
		return arguments.length === 1 ? this.off( selector, "**" ) : this.off( types, selector || "**", fn );
	}
});


var nonce = jQuery.now();

var rquery = (/\?/);



// Support: Android 2.3
// Workaround failure to string-cast null input
jQuery.parseJSON = function( data ) {
	return JSON.parse( data + "" );
};


// Cross-browser xml parsing
jQuery.parseXML = function( data ) {
	var xml, tmp;
	if ( !data || typeof data !== "string" ) {
		return null;
	}

	// Support: IE9
	try {
		tmp = new DOMParser();
		xml = tmp.parseFromString( data, "text/xml" );
	} catch ( e ) {
		xml = undefined;
	}

	if ( !xml || xml.getElementsByTagName( "parsererror" ).length ) {
		jQuery.error( "Invalid XML: " + data );
	}
	return xml;
};


var
	rhash = /#.*$/,
	rts = /([?&])_=[^&]*/,
	rheaders = /^(.*?):[ \t]*([^\r\n]*)$/mg,
	// #7653, #8125, #8152: local protocol detection
	rlocalProtocol = /^(?:about|app|app-storage|.+-extension|file|res|widget):$/,
	rnoContent = /^(?:GET|HEAD)$/,
	rprotocol = /^\/\//,
	rurl = /^([\w.+-]+:)(?:\/\/(?:[^\/?#]*@|)([^\/?#:]*)(?::(\d+)|)|)/,

	/* Prefilters
	 * 1) They are useful to introduce custom dataTypes (see ajax/jsonp.js for an example)
	 * 2) These are called:
	 *    - BEFORE asking for a transport
	 *    - AFTER param serialization (s.data is a string if s.processData is true)
	 * 3) key is the dataType
	 * 4) the catchall symbol "*" can be used
	 * 5) execution will start with transport dataType and THEN continue down to "*" if needed
	 */
	prefilters = {},

	/* Transports bindings
	 * 1) key is the dataType
	 * 2) the catchall symbol "*" can be used
	 * 3) selection will start with transport dataType and THEN go to "*" if needed
	 */
	transports = {},

	// Avoid comment-prolog char sequence (#10098); must appease lint and evade compression
	allTypes = "*/".concat( "*" ),

	// Document location
	ajaxLocation = window.location.href,

	// Segment location into parts
	ajaxLocParts = rurl.exec( ajaxLocation.toLowerCase() ) || [];

// Base "constructor" for jQuery.ajaxPrefilter and jQuery.ajaxTransport
function addToPrefiltersOrTransports( structure ) {

	// dataTypeExpression is optional and defaults to "*"
	return function( dataTypeExpression, func ) {

		if ( typeof dataTypeExpression !== "string" ) {
			func = dataTypeExpression;
			dataTypeExpression = "*";
		}

		var dataType,
			i = 0,
			dataTypes = dataTypeExpression.toLowerCase().match( rnotwhite ) || [];

		if ( jQuery.isFunction( func ) ) {
			// For each dataType in the dataTypeExpression
			while ( (dataType = dataTypes[i++]) ) {
				// Prepend if requested
				if ( dataType[0] === "+" ) {
					dataType = dataType.slice( 1 ) || "*";
					(structure[ dataType ] = structure[ dataType ] || []).unshift( func );

				// Otherwise append
				} else {
					(structure[ dataType ] = structure[ dataType ] || []).push( func );
				}
			}
		}
	};
}

// Base inspection function for prefilters and transports
function inspectPrefiltersOrTransports( structure, options, originalOptions, jqXHR ) {

	var inspected = {},
		seekingTransport = ( structure === transports );

	function inspect( dataType ) {
		var selected;
		inspected[ dataType ] = true;
		jQuery.each( structure[ dataType ] || [], function( _, prefilterOrFactory ) {
			var dataTypeOrTransport = prefilterOrFactory( options, originalOptions, jqXHR );
			if ( typeof dataTypeOrTransport === "string" && !seekingTransport && !inspected[ dataTypeOrTransport ] ) {
				options.dataTypes.unshift( dataTypeOrTransport );
				inspect( dataTypeOrTransport );
				return false;
			} else if ( seekingTransport ) {
				return !( selected = dataTypeOrTransport );
			}
		});
		return selected;
	}

	return inspect( options.dataTypes[ 0 ] ) || !inspected[ "*" ] && inspect( "*" );
}

// A special extend for ajax options
// that takes "flat" options (not to be deep extended)
// Fixes #9887
function ajaxExtend( target, src ) {
	var key, deep,
		flatOptions = jQuery.ajaxSettings.flatOptions || {};

	for ( key in src ) {
		if ( src[ key ] !== undefined ) {
			( flatOptions[ key ] ? target : ( deep || (deep = {}) ) )[ key ] = src[ key ];
		}
	}
	if ( deep ) {
		jQuery.extend( true, target, deep );
	}

	return target;
}

/* Handles responses to an ajax request:
 * - finds the right dataType (mediates between content-type and expected dataType)
 * - returns the corresponding response
 */
function ajaxHandleResponses( s, jqXHR, responses ) {

	var ct, type, finalDataType, firstDataType,
		contents = s.contents,
		dataTypes = s.dataTypes;

	// Remove auto dataType and get content-type in the process
	while ( dataTypes[ 0 ] === "*" ) {
		dataTypes.shift();
		if ( ct === undefined ) {
			ct = s.mimeType || jqXHR.getResponseHeader("Content-Type");
		}
	}

	// Check if we're dealing with a known content-type
	if ( ct ) {
		for ( type in contents ) {
			if ( contents[ type ] && contents[ type ].test( ct ) ) {
				dataTypes.unshift( type );
				break;
			}
		}
	}

	// Check to see if we have a response for the expected dataType
	if ( dataTypes[ 0 ] in responses ) {
		finalDataType = dataTypes[ 0 ];
	} else {
		// Try convertible dataTypes
		for ( type in responses ) {
			if ( !dataTypes[ 0 ] || s.converters[ type + " " + dataTypes[0] ] ) {
				finalDataType = type;
				break;
			}
			if ( !firstDataType ) {
				firstDataType = type;
			}
		}
		// Or just use first one
		finalDataType = finalDataType || firstDataType;
	}

	// If we found a dataType
	// We add the dataType to the list if needed
	// and return the corresponding response
	if ( finalDataType ) {
		if ( finalDataType !== dataTypes[ 0 ] ) {
			dataTypes.unshift( finalDataType );
		}
		return responses[ finalDataType ];
	}
}

/* Chain conversions given the request and the original response
 * Also sets the responseXXX fields on the jqXHR instance
 */
function ajaxConvert( s, response, jqXHR, isSuccess ) {
	var conv2, current, conv, tmp, prev,
		converters = {},
		// Work with a copy of dataTypes in case we need to modify it for conversion
		dataTypes = s.dataTypes.slice();

	// Create converters map with lowercased keys
	if ( dataTypes[ 1 ] ) {
		for ( conv in s.converters ) {
			converters[ conv.toLowerCase() ] = s.converters[ conv ];
		}
	}

	current = dataTypes.shift();

	// Convert to each sequential dataType
	while ( current ) {

		if ( s.responseFields[ current ] ) {
			jqXHR[ s.responseFields[ current ] ] = response;
		}

		// Apply the dataFilter if provided
		if ( !prev && isSuccess && s.dataFilter ) {
			response = s.dataFilter( response, s.dataType );
		}

		prev = current;
		current = dataTypes.shift();

		if ( current ) {

		// There's only work to do if current dataType is non-auto
			if ( current === "*" ) {

				current = prev;

			// Convert response if prev dataType is non-auto and differs from current
			} else if ( prev !== "*" && prev !== current ) {

				// Seek a direct converter
				conv = converters[ prev + " " + current ] || converters[ "* " + current ];

				// If none found, seek a pair
				if ( !conv ) {
					for ( conv2 in converters ) {

						// If conv2 outputs current
						tmp = conv2.split( " " );
						if ( tmp[ 1 ] === current ) {

							// If prev can be converted to accepted input
							conv = converters[ prev + " " + tmp[ 0 ] ] ||
								converters[ "* " + tmp[ 0 ] ];
							if ( conv ) {
								// Condense equivalence converters
								if ( conv === true ) {
									conv = converters[ conv2 ];

								// Otherwise, insert the intermediate dataType
								} else if ( converters[ conv2 ] !== true ) {
									current = tmp[ 0 ];
									dataTypes.unshift( tmp[ 1 ] );
								}
								break;
							}
						}
					}
				}

				// Apply converter (if not an equivalence)
				if ( conv !== true ) {

					// Unless errors are allowed to bubble, catch and return them
					if ( conv && s[ "throws" ] ) {
						response = conv( response );
					} else {
						try {
							response = conv( response );
						} catch ( e ) {
							return { state: "parsererror", error: conv ? e : "No conversion from " + prev + " to " + current };
						}
					}
				}
			}
		}
	}

	return { state: "success", data: response };
}

jQuery.extend({

	// Counter for holding the number of active queries
	active: 0,

	// Last-Modified header cache for next request
	lastModified: {},
	etag: {},

	ajaxSettings: {
		url: ajaxLocation,
		type: "GET",
		isLocal: rlocalProtocol.test( ajaxLocParts[ 1 ] ),
		global: true,
		processData: true,
		async: true,
		contentType: "application/x-www-form-urlencoded; charset=UTF-8",
		/*
		timeout: 0,
		data: null,
		dataType: null,
		username: null,
		password: null,
		cache: null,
		throws: false,
		traditional: false,
		headers: {},
		*/

		accepts: {
			"*": allTypes,
			text: "text/plain",
			html: "text/html",
			xml: "application/xml, text/xml",
			json: "application/json, text/javascript"
		},

		contents: {
			xml: /xml/,
			html: /html/,
			json: /json/
		},

		responseFields: {
			xml: "responseXML",
			text: "responseText",
			json: "responseJSON"
		},

		// Data converters
		// Keys separate source (or catchall "*") and destination types with a single space
		converters: {

			// Convert anything to text
			"* text": String,

			// Text to html (true = no transformation)
			"text html": true,

			// Evaluate text as a json expression
			"text json": jQuery.parseJSON,

			// Parse text as xml
			"text xml": jQuery.parseXML
		},

		// For options that shouldn't be deep extended:
		// you can add your own custom options here if
		// and when you create one that shouldn't be
		// deep extended (see ajaxExtend)
		flatOptions: {
			url: true,
			context: true
		}
	},

	// Creates a full fledged settings object into target
	// with both ajaxSettings and settings fields.
	// If target is omitted, writes into ajaxSettings.
	ajaxSetup: function( target, settings ) {
		return settings ?

			// Building a settings object
			ajaxExtend( ajaxExtend( target, jQuery.ajaxSettings ), settings ) :

			// Extending ajaxSettings
			ajaxExtend( jQuery.ajaxSettings, target );
	},

	ajaxPrefilter: addToPrefiltersOrTransports( prefilters ),
	ajaxTransport: addToPrefiltersOrTransports( transports ),

	// Main method
	ajax: function( url, options ) {

		// If url is an object, simulate pre-1.5 signature
		if ( typeof url === "object" ) {
			options = url;
			url = undefined;
		}

		// Force options to be an object
		options = options || {};

		var transport,
			// URL without anti-cache param
			cacheURL,
			// Response headers
			responseHeadersString,
			responseHeaders,
			// timeout handle
			timeoutTimer,
			// Cross-domain detection vars
			parts,
			// To know if global events are to be dispatched
			fireGlobals,
			// Loop variable
			i,
			// Create the final options object
			s = jQuery.ajaxSetup( {}, options ),
			// Callbacks context
			callbackContext = s.context || s,
			// Context for global events is callbackContext if it is a DOM node or jQuery collection
			globalEventContext = s.context && ( callbackContext.nodeType || callbackContext.jquery ) ?
				jQuery( callbackContext ) :
				jQuery.event,
			// Deferreds
			deferred = jQuery.Deferred(),
			completeDeferred = jQuery.Callbacks("once memory"),
			// Status-dependent callbacks
			statusCode = s.statusCode || {},
			// Headers (they are sent all at once)
			requestHeaders = {},
			requestHeadersNames = {},
			// The jqXHR state
			state = 0,
			// Default abort message
			strAbort = "canceled",
			// Fake xhr
			jqXHR = {
				readyState: 0,

				// Builds headers hashtable if needed
				getResponseHeader: function( key ) {
					var match;
					if ( state === 2 ) {
						if ( !responseHeaders ) {
							responseHeaders = {};
							while ( (match = rheaders.exec( responseHeadersString )) ) {
								responseHeaders[ match[1].toLowerCase() ] = match[ 2 ];
							}
						}
						match = responseHeaders[ key.toLowerCase() ];
					}
					return match == null ? null : match;
				},

				// Raw string
				getAllResponseHeaders: function() {
					return state === 2 ? responseHeadersString : null;
				},

				// Caches the header
				setRequestHeader: function( name, value ) {
					var lname = name.toLowerCase();
					if ( !state ) {
						name = requestHeadersNames[ lname ] = requestHeadersNames[ lname ] || name;
						requestHeaders[ name ] = value;
					}
					return this;
				},

				// Overrides response content-type header
				overrideMimeType: function( type ) {
					if ( !state ) {
						s.mimeType = type;
					}
					return this;
				},

				// Status-dependent callbacks
				statusCode: function( map ) {
					var code;
					if ( map ) {
						if ( state < 2 ) {
							for ( code in map ) {
								// Lazy-add the new callback in a way that preserves old ones
								statusCode[ code ] = [ statusCode[ code ], map[ code ] ];
							}
						} else {
							// Execute the appropriate callbacks
							jqXHR.always( map[ jqXHR.status ] );
						}
					}
					return this;
				},

				// Cancel the request
				abort: function( statusText ) {
					var finalText = statusText || strAbort;
					if ( transport ) {
						transport.abort( finalText );
					}
					done( 0, finalText );
					return this;
				}
			};

		// Attach deferreds
		deferred.promise( jqXHR ).complete = completeDeferred.add;
		jqXHR.success = jqXHR.done;
		jqXHR.error = jqXHR.fail;

		// Remove hash character (#7531: and string promotion)
		// Add protocol if not provided (prefilters might expect it)
		// Handle falsy url in the settings object (#10093: consistency with old signature)
		// We also use the url parameter if available
		s.url = ( ( url || s.url || ajaxLocation ) + "" ).replace( rhash, "" )
			.replace( rprotocol, ajaxLocParts[ 1 ] + "//" );

		// Alias method option to type as per ticket #12004
		s.type = options.method || options.type || s.method || s.type;

		// Extract dataTypes list
		s.dataTypes = jQuery.trim( s.dataType || "*" ).toLowerCase().match( rnotwhite ) || [ "" ];

		// A cross-domain request is in order when we have a protocol:host:port mismatch
		if ( s.crossDomain == null ) {
			parts = rurl.exec( s.url.toLowerCase() );
			s.crossDomain = !!( parts &&
				( parts[ 1 ] !== ajaxLocParts[ 1 ] || parts[ 2 ] !== ajaxLocParts[ 2 ] ||
					( parts[ 3 ] || ( parts[ 1 ] === "http:" ? "80" : "443" ) ) !==
						( ajaxLocParts[ 3 ] || ( ajaxLocParts[ 1 ] === "http:" ? "80" : "443" ) ) )
			);
		}

		// Convert data if not already a string
		if ( s.data && s.processData && typeof s.data !== "string" ) {
			s.data = jQuery.param( s.data, s.traditional );
		}

		// Apply prefilters
		inspectPrefiltersOrTransports( prefilters, s, options, jqXHR );

		// If request was aborted inside a prefilter, stop there
		if ( state === 2 ) {
			return jqXHR;
		}

		// We can fire global events as of now if asked to
		// Don't fire events if jQuery.event is undefined in an AMD-usage scenario (#15118)
		fireGlobals = jQuery.event && s.global;

		// Watch for a new set of requests
		if ( fireGlobals && jQuery.active++ === 0 ) {
			jQuery.event.trigger("ajaxStart");
		}

		// Uppercase the type
		s.type = s.type.toUpperCase();

		// Determine if request has content
		s.hasContent = !rnoContent.test( s.type );

		// Save the URL in case we're toying with the If-Modified-Since
		// and/or If-None-Match header later on
		cacheURL = s.url;

		// More options handling for requests with no content
		if ( !s.hasContent ) {

			// If data is available, append data to url
			if ( s.data ) {
				cacheURL = ( s.url += ( rquery.test( cacheURL ) ? "&" : "?" ) + s.data );
				// #9682: remove data so that it's not used in an eventual retry
				delete s.data;
			}

			// Add anti-cache in url if needed
			if ( s.cache === false ) {
				s.url = rts.test( cacheURL ) ?

					// If there is already a '_' parameter, set its value
					cacheURL.replace( rts, "$1_=" + nonce++ ) :

					// Otherwise add one to the end
					cacheURL + ( rquery.test( cacheURL ) ? "&" : "?" ) + "_=" + nonce++;
			}
		}

		// Set the If-Modified-Since and/or If-None-Match header, if in ifModified mode.
		if ( s.ifModified ) {
			if ( jQuery.lastModified[ cacheURL ] ) {
				jqXHR.setRequestHeader( "If-Modified-Since", jQuery.lastModified[ cacheURL ] );
			}
			if ( jQuery.etag[ cacheURL ] ) {
				jqXHR.setRequestHeader( "If-None-Match", jQuery.etag[ cacheURL ] );
			}
		}

		// Set the correct header, if data is being sent
		if ( s.data && s.hasContent && s.contentType !== false || options.contentType ) {
			jqXHR.setRequestHeader( "Content-Type", s.contentType );
		}

		// Set the Accepts header for the server, depending on the dataType
		jqXHR.setRequestHeader(
			"Accept",
			s.dataTypes[ 0 ] && s.accepts[ s.dataTypes[0] ] ?
				s.accepts[ s.dataTypes[0] ] + ( s.dataTypes[ 0 ] !== "*" ? ", " + allTypes + "; q=0.01" : "" ) :
				s.accepts[ "*" ]
		);

		// Check for headers option
		for ( i in s.headers ) {
			jqXHR.setRequestHeader( i, s.headers[ i ] );
		}

		// Allow custom headers/mimetypes and early abort
		if ( s.beforeSend && ( s.beforeSend.call( callbackContext, jqXHR, s ) === false || state === 2 ) ) {
			// Abort if not done already and return
			return jqXHR.abort();
		}

		// Aborting is no longer a cancellation
		strAbort = "abort";

		// Install callbacks on deferreds
		for ( i in { success: 1, error: 1, complete: 1 } ) {
			jqXHR[ i ]( s[ i ] );
		}

		// Get transport
		transport = inspectPrefiltersOrTransports( transports, s, options, jqXHR );

		// If no transport, we auto-abort
		if ( !transport ) {
			done( -1, "No Transport" );
		} else {
			jqXHR.readyState = 1;

			// Send global event
			if ( fireGlobals ) {
				globalEventContext.trigger( "ajaxSend", [ jqXHR, s ] );
			}
			// Timeout
			if ( s.async && s.timeout > 0 ) {
				timeoutTimer = setTimeout(function() {
					jqXHR.abort("timeout");
				}, s.timeout );
			}

			try {
				state = 1;
				transport.send( requestHeaders, done );
			} catch ( e ) {
				// Propagate exception as error if not done
				if ( state < 2 ) {
					done( -1, e );
				// Simply rethrow otherwise
				} else {
					throw e;
				}
			}
		}

		// Callback for when everything is done
		function done( status, nativeStatusText, responses, headers ) {
			var isSuccess, success, error, response, modified,
				statusText = nativeStatusText;

			// Called once
			if ( state === 2 ) {
				return;
			}

			// State is "done" now
			state = 2;

			// Clear timeout if it exists
			if ( timeoutTimer ) {
				clearTimeout( timeoutTimer );
			}

			// Dereference transport for early garbage collection
			// (no matter how long the jqXHR object will be used)
			transport = undefined;

			// Cache response headers
			responseHeadersString = headers || "";

			// Set readyState
			jqXHR.readyState = status > 0 ? 4 : 0;

			// Determine if successful
			isSuccess = status >= 200 && status < 300 || status === 304;

			// Get response data
			if ( responses ) {
				response = ajaxHandleResponses( s, jqXHR, responses );
			}

			// Convert no matter what (that way responseXXX fields are always set)
			response = ajaxConvert( s, response, jqXHR, isSuccess );

			// If successful, handle type chaining
			if ( isSuccess ) {

				// Set the If-Modified-Since and/or If-None-Match header, if in ifModified mode.
				if ( s.ifModified ) {
					modified = jqXHR.getResponseHeader("Last-Modified");
					if ( modified ) {
						jQuery.lastModified[ cacheURL ] = modified;
					}
					modified = jqXHR.getResponseHeader("etag");
					if ( modified ) {
						jQuery.etag[ cacheURL ] = modified;
					}
				}

				// if no content
				if ( status === 204 || s.type === "HEAD" ) {
					statusText = "nocontent";

				// if not modified
				} else if ( status === 304 ) {
					statusText = "notmodified";

				// If we have data, let's convert it
				} else {
					statusText = response.state;
					success = response.data;
					error = response.error;
					isSuccess = !error;
				}
			} else {
				// Extract error from statusText and normalize for non-aborts
				error = statusText;
				if ( status || !statusText ) {
					statusText = "error";
					if ( status < 0 ) {
						status = 0;
					}
				}
			}

			// Set data for the fake xhr object
			jqXHR.status = status;
			jqXHR.statusText = ( nativeStatusText || statusText ) + "";

			// Success/Error
			if ( isSuccess ) {
				deferred.resolveWith( callbackContext, [ success, statusText, jqXHR ] );
			} else {
				deferred.rejectWith( callbackContext, [ jqXHR, statusText, error ] );
			}

			// Status-dependent callbacks
			jqXHR.statusCode( statusCode );
			statusCode = undefined;

			if ( fireGlobals ) {
				globalEventContext.trigger( isSuccess ? "ajaxSuccess" : "ajaxError",
					[ jqXHR, s, isSuccess ? success : error ] );
			}

			// Complete
			completeDeferred.fireWith( callbackContext, [ jqXHR, statusText ] );

			if ( fireGlobals ) {
				globalEventContext.trigger( "ajaxComplete", [ jqXHR, s ] );
				// Handle the global AJAX counter
				if ( !( --jQuery.active ) ) {
					jQuery.event.trigger("ajaxStop");
				}
			}
		}

		return jqXHR;
	},

	getJSON: function( url, data, callback ) {
		return jQuery.get( url, data, callback, "json" );
	},

	getScript: function( url, callback ) {
		return jQuery.get( url, undefined, callback, "script" );
	}
});

jQuery.each( [ "get", "post" ], function( i, method ) {
	jQuery[ method ] = function( url, data, callback, type ) {
		// Shift arguments if data argument was omitted
		if ( jQuery.isFunction( data ) ) {
			type = type || callback;
			callback = data;
			data = undefined;
		}

		return jQuery.ajax({
			url: url,
			type: method,
			dataType: type,
			data: data,
			success: callback
		});
	};
});


jQuery._evalUrl = function( url ) {
	return jQuery.ajax({
		url: url,
		type: "GET",
		dataType: "script",
		async: false,
		global: false,
		"throws": true
	});
};


jQuery.fn.extend({
	wrapAll: function( html ) {
		var wrap;

		if ( jQuery.isFunction( html ) ) {
			return this.each(function( i ) {
				jQuery( this ).wrapAll( html.call(this, i) );
			});
		}

		if ( this[ 0 ] ) {

			// The elements to wrap the target around
			wrap = jQuery( html, this[ 0 ].ownerDocument ).eq( 0 ).clone( true );

			if ( this[ 0 ].parentNode ) {
				wrap.insertBefore( this[ 0 ] );
			}

			wrap.map(function() {
				var elem = this;

				while ( elem.firstElementChild ) {
					elem = elem.firstElementChild;
				}

				return elem;
			}).append( this );
		}

		return this;
	},

	wrapInner: function( html ) {
		if ( jQuery.isFunction( html ) ) {
			return this.each(function( i ) {
				jQuery( this ).wrapInner( html.call(this, i) );
			});
		}

		return this.each(function() {
			var self = jQuery( this ),
				contents = self.contents();

			if ( contents.length ) {
				contents.wrapAll( html );

			} else {
				self.append( html );
			}
		});
	},

	wrap: function( html ) {
		var isFunction = jQuery.isFunction( html );

		return this.each(function( i ) {
			jQuery( this ).wrapAll( isFunction ? html.call(this, i) : html );
		});
	},

	unwrap: function() {
		return this.parent().each(function() {
			if ( !jQuery.nodeName( this, "body" ) ) {
				jQuery( this ).replaceWith( this.childNodes );
			}
		}).end();
	}
});


jQuery.expr.filters.hidden = function( elem ) {
	// Support: Opera <= 12.12
	// Opera reports offsetWidths and offsetHeights less than zero on some elements
	return elem.offsetWidth <= 0 && elem.offsetHeight <= 0;
};
jQuery.expr.filters.visible = function( elem ) {
	return !jQuery.expr.filters.hidden( elem );
};




var r20 = /%20/g,
	rbracket = /\[\]$/,
	rCRLF = /\r?\n/g,
	rsubmitterTypes = /^(?:submit|button|image|reset|file)$/i,
	rsubmittable = /^(?:input|select|textarea|keygen)/i;

function buildParams( prefix, obj, traditional, add ) {
	var name;

	if ( jQuery.isArray( obj ) ) {
		// Serialize array item.
		jQuery.each( obj, function( i, v ) {
			if ( traditional || rbracket.test( prefix ) ) {
				// Treat each array item as a scalar.
				add( prefix, v );

			} else {
				// Item is non-scalar (array or object), encode its numeric index.
				buildParams( prefix + "[" + ( typeof v === "object" ? i : "" ) + "]", v, traditional, add );
			}
		});

	} else if ( !traditional && jQuery.type( obj ) === "object" ) {
		// Serialize object item.
		for ( name in obj ) {
			buildParams( prefix + "[" + name + "]", obj[ name ], traditional, add );
		}

	} else {
		// Serialize scalar item.
		add( prefix, obj );
	}
}

// Serialize an array of form elements or a set of
// key/values into a query string
jQuery.param = function( a, traditional ) {
	var prefix,
		s = [],
		add = function( key, value ) {
			// If value is a function, invoke it and return its value
			value = jQuery.isFunction( value ) ? value() : ( value == null ? "" : value );
			s[ s.length ] = encodeURIComponent( key ) + "=" + encodeURIComponent( value );
		};

	// Set traditional to true for jQuery <= 1.3.2 behavior.
	if ( traditional === undefined ) {
		traditional = jQuery.ajaxSettings && jQuery.ajaxSettings.traditional;
	}

	// If an array was passed in, assume that it is an array of form elements.
	if ( jQuery.isArray( a ) || ( a.jquery && !jQuery.isPlainObject( a ) ) ) {
		// Serialize the form elements
		jQuery.each( a, function() {
			add( this.name, this.value );
		});

	} else {
		// If traditional, encode the "old" way (the way 1.3.2 or older
		// did it), otherwise encode params recursively.
		for ( prefix in a ) {
			buildParams( prefix, a[ prefix ], traditional, add );
		}
	}

	// Return the resulting serialization
	return s.join( "&" ).replace( r20, "+" );
};

jQuery.fn.extend({
	serialize: function() {
		return jQuery.param( this.serializeArray() );
	},
	serializeArray: function() {
		return this.map(function() {
			// Can add propHook for "elements" to filter or add form elements
			var elements = jQuery.prop( this, "elements" );
			return elements ? jQuery.makeArray( elements ) : this;
		})
		.filter(function() {
			var type = this.type;

			// Use .is( ":disabled" ) so that fieldset[disabled] works
			return this.name && !jQuery( this ).is( ":disabled" ) &&
				rsubmittable.test( this.nodeName ) && !rsubmitterTypes.test( type ) &&
				( this.checked || !rcheckableType.test( type ) );
		})
		.map(function( i, elem ) {
			var val = jQuery( this ).val();

			return val == null ?
				null :
				jQuery.isArray( val ) ?
					jQuery.map( val, function( val ) {
						return { name: elem.name, value: val.replace( rCRLF, "\r\n" ) };
					}) :
					{ name: elem.name, value: val.replace( rCRLF, "\r\n" ) };
		}).get();
	}
});


jQuery.ajaxSettings.xhr = function() {
	try {
		return new XMLHttpRequest();
	} catch( e ) {}
};

var xhrId = 0,
	xhrCallbacks = {},
	xhrSuccessStatus = {
		// file protocol always yields status code 0, assume 200
		0: 200,
		// Support: IE9
		// #1450: sometimes IE returns 1223 when it should be 204
		1223: 204
	},
	xhrSupported = jQuery.ajaxSettings.xhr();

// Support: IE9
// Open requests must be manually aborted on unload (#5280)
// See https://support.microsoft.com/kb/2856746 for more info
if ( window.attachEvent ) {
	window.attachEvent( "onunload", function() {
		for ( var key in xhrCallbacks ) {
			xhrCallbacks[ key ]();
		}
	});
}

support.cors = !!xhrSupported && ( "withCredentials" in xhrSupported );
support.ajax = xhrSupported = !!xhrSupported;

jQuery.ajaxTransport(function( options ) {
	var callback;

	// Cross domain only allowed if supported through XMLHttpRequest
	if ( support.cors || xhrSupported && !options.crossDomain ) {
		return {
			send: function( headers, complete ) {
				var i,
					xhr = options.xhr(),
					id = ++xhrId;

				xhr.open( options.type, options.url, options.async, options.username, options.password );

				// Apply custom fields if provided
				if ( options.xhrFields ) {
					for ( i in options.xhrFields ) {
						xhr[ i ] = options.xhrFields[ i ];
					}
				}

				// Override mime type if needed
				if ( options.mimeType && xhr.overrideMimeType ) {
					xhr.overrideMimeType( options.mimeType );
				}

				// X-Requested-With header
				// For cross-domain requests, seeing as conditions for a preflight are
				// akin to a jigsaw puzzle, we simply never set it to be sure.
				// (it can always be set on a per-request basis or even using ajaxSetup)
				// For same-domain requests, won't change header if already provided.
				if ( !options.crossDomain && !headers["X-Requested-With"] ) {
					headers["X-Requested-With"] = "XMLHttpRequest";
				}

				// Set headers
				for ( i in headers ) {
					xhr.setRequestHeader( i, headers[ i ] );
				}

				// Callback
				callback = function( type ) {
					return function() {
						if ( callback ) {
							delete xhrCallbacks[ id ];
							callback = xhr.onload = xhr.onerror = null;

							if ( type === "abort" ) {
								xhr.abort();
							} else if ( type === "error" ) {
								complete(
									// file: protocol always yields status 0; see #8605, #14207
									xhr.status,
									xhr.statusText
								);
							} else {
								complete(
									xhrSuccessStatus[ xhr.status ] || xhr.status,
									xhr.statusText,
									// Support: IE9
									// Accessing binary-data responseText throws an exception
									// (#11426)
									typeof xhr.responseText === "string" ? {
										text: xhr.responseText
									} : undefined,
									xhr.getAllResponseHeaders()
								);
							}
						}
					};
				};

				// Listen to events
				xhr.onload = callback();
				xhr.onerror = callback("error");

				// Create the abort callback
				callback = xhrCallbacks[ id ] = callback("abort");

				try {
					// Do send the request (this may raise an exception)
					xhr.send( options.hasContent && options.data || null );
				} catch ( e ) {
					// #14683: Only rethrow if this hasn't been notified as an error yet
					if ( callback ) {
						throw e;
					}
				}
			},

			abort: function() {
				if ( callback ) {
					callback();
				}
			}
		};
	}
});




// Install script dataType
jQuery.ajaxSetup({
	accepts: {
		script: "text/javascript, application/javascript, application/ecmascript, application/x-ecmascript"
	},
	contents: {
		script: /(?:java|ecma)script/
	},
	converters: {
		"text script": function( text ) {
			jQuery.globalEval( text );
			return text;
		}
	}
});

// Handle cache's special case and crossDomain
jQuery.ajaxPrefilter( "script", function( s ) {
	if ( s.cache === undefined ) {
		s.cache = false;
	}
	if ( s.crossDomain ) {
		s.type = "GET";
	}
});

// Bind script tag hack transport
jQuery.ajaxTransport( "script", function( s ) {
	// This transport only deals with cross domain requests
	if ( s.crossDomain ) {
		var script, callback;
		return {
			send: function( _, complete ) {
				script = jQuery("<script>").prop({
					async: true,
					charset: s.scriptCharset,
					src: s.url
				}).on(
					"load error",
					callback = function( evt ) {
						script.remove();
						callback = null;
						if ( evt ) {
							complete( evt.type === "error" ? 404 : 200, evt.type );
						}
					}
				);
				document.head.appendChild( script[ 0 ] );
			},
			abort: function() {
				if ( callback ) {
					callback();
				}
			}
		};
	}
});




var oldCallbacks = [],
	rjsonp = /(=)\?(?=&|$)|\?\?/;

// Default jsonp settings
jQuery.ajaxSetup({
	jsonp: "callback",
	jsonpCallback: function() {
		var callback = oldCallbacks.pop() || ( jQuery.expando + "_" + ( nonce++ ) );
		this[ callback ] = true;
		return callback;
	}
});

// Detect, normalize options and install callbacks for jsonp requests
jQuery.ajaxPrefilter( "json jsonp", function( s, originalSettings, jqXHR ) {

	var callbackName, overwritten, responseContainer,
		jsonProp = s.jsonp !== false && ( rjsonp.test( s.url ) ?
			"url" :
			typeof s.data === "string" && !( s.contentType || "" ).indexOf("application/x-www-form-urlencoded") && rjsonp.test( s.data ) && "data"
		);

	// Handle iff the expected data type is "jsonp" or we have a parameter to set
	if ( jsonProp || s.dataTypes[ 0 ] === "jsonp" ) {

		// Get callback name, remembering preexisting value associated with it
		callbackName = s.jsonpCallback = jQuery.isFunction( s.jsonpCallback ) ?
			s.jsonpCallback() :
			s.jsonpCallback;

		// Insert callback into url or form data
		if ( jsonProp ) {
			s[ jsonProp ] = s[ jsonProp ].replace( rjsonp, "$1" + callbackName );
		} else if ( s.jsonp !== false ) {
			s.url += ( rquery.test( s.url ) ? "&" : "?" ) + s.jsonp + "=" + callbackName;
		}

		// Use data converter to retrieve json after script execution
		s.converters["script json"] = function() {
			if ( !responseContainer ) {
				jQuery.error( callbackName + " was not called" );
			}
			return responseContainer[ 0 ];
		};

		// force json dataType
		s.dataTypes[ 0 ] = "json";

		// Install callback
		overwritten = window[ callbackName ];
		window[ callbackName ] = function() {
			responseContainer = arguments;
		};

		// Clean-up function (fires after converters)
		jqXHR.always(function() {
			// Restore preexisting value
			window[ callbackName ] = overwritten;

			// Save back as free
			if ( s[ callbackName ] ) {
				// make sure that re-using the options doesn't screw things around
				s.jsonpCallback = originalSettings.jsonpCallback;

				// save the callback name for future use
				oldCallbacks.push( callbackName );
			}

			// Call if it was a function and we have a response
			if ( responseContainer && jQuery.isFunction( overwritten ) ) {
				overwritten( responseContainer[ 0 ] );
			}

			responseContainer = overwritten = undefined;
		});

		// Delegate to script
		return "script";
	}
});




// data: string of html
// context (optional): If specified, the fragment will be created in this context, defaults to document
// keepScripts (optional): If true, will include scripts passed in the html string
jQuery.parseHTML = function( data, context, keepScripts ) {
	if ( !data || typeof data !== "string" ) {
		return null;
	}
	if ( typeof context === "boolean" ) {
		keepScripts = context;
		context = false;
	}
	context = context || document;

	var parsed = rsingleTag.exec( data ),
		scripts = !keepScripts && [];

	// Single tag
	if ( parsed ) {
		return [ context.createElement( parsed[1] ) ];
	}

	parsed = jQuery.buildFragment( [ data ], context, scripts );

	if ( scripts && scripts.length ) {
		jQuery( scripts ).remove();
	}

	return jQuery.merge( [], parsed.childNodes );
};


// Keep a copy of the old load method
var _load = jQuery.fn.load;

/**
 * Load a url into a page
 */
jQuery.fn.load = function( url, params, callback ) {
	if ( typeof url !== "string" && _load ) {
		return _load.apply( this, arguments );
	}

	var selector, type, response,
		self = this,
		off = url.indexOf(" ");

	if ( off >= 0 ) {
		selector = jQuery.trim( url.slice( off ) );
		url = url.slice( 0, off );
	}

	// If it's a function
	if ( jQuery.isFunction( params ) ) {

		// We assume that it's the callback
		callback = params;
		params = undefined;

	// Otherwise, build a param string
	} else if ( params && typeof params === "object" ) {
		type = "POST";
	}

	// If we have elements to modify, make the request
	if ( self.length > 0 ) {
		jQuery.ajax({
			url: url,

			// if "type" variable is undefined, then "GET" method will be used
			type: type,
			dataType: "html",
			data: params
		}).done(function( responseText ) {

			// Save response for use in complete callback
			response = arguments;

			self.html( selector ?

				// If a selector was specified, locate the right elements in a dummy div
				// Exclude scripts to avoid IE 'Permission Denied' errors
				jQuery("<div>").append( jQuery.parseHTML( responseText ) ).find( selector ) :

				// Otherwise use the full result
				responseText );

		}).complete( callback && function( jqXHR, status ) {
			self.each( callback, response || [ jqXHR.responseText, status, jqXHR ] );
		});
	}

	return this;
};




// Attach a bunch of functions for handling common AJAX events
jQuery.each( [ "ajaxStart", "ajaxStop", "ajaxComplete", "ajaxError", "ajaxSuccess", "ajaxSend" ], function( i, type ) {
	jQuery.fn[ type ] = function( fn ) {
		return this.on( type, fn );
	};
});




jQuery.expr.filters.animated = function( elem ) {
	return jQuery.grep(jQuery.timers, function( fn ) {
		return elem === fn.elem;
	}).length;
};




var docElem = window.document.documentElement;

/**
 * Gets a window from an element
 */
function getWindow( elem ) {
	return jQuery.isWindow( elem ) ? elem : elem.nodeType === 9 && elem.defaultView;
}

jQuery.offset = {
	setOffset: function( elem, options, i ) {
		var curPosition, curLeft, curCSSTop, curTop, curOffset, curCSSLeft, calculatePosition,
			position = jQuery.css( elem, "position" ),
			curElem = jQuery( elem ),
			props = {};

		// Set position first, in-case top/left are set even on static elem
		if ( position === "static" ) {
			elem.style.position = "relative";
		}

		curOffset = curElem.offset();
		curCSSTop = jQuery.css( elem, "top" );
		curCSSLeft = jQuery.css( elem, "left" );
		calculatePosition = ( position === "absolute" || position === "fixed" ) &&
			( curCSSTop + curCSSLeft ).indexOf("auto") > -1;

		// Need to be able to calculate position if either
		// top or left is auto and position is either absolute or fixed
		if ( calculatePosition ) {
			curPosition = curElem.position();
			curTop = curPosition.top;
			curLeft = curPosition.left;

		} else {
			curTop = parseFloat( curCSSTop ) || 0;
			curLeft = parseFloat( curCSSLeft ) || 0;
		}

		if ( jQuery.isFunction( options ) ) {
			options = options.call( elem, i, curOffset );
		}

		if ( options.top != null ) {
			props.top = ( options.top - curOffset.top ) + curTop;
		}
		if ( options.left != null ) {
			props.left = ( options.left - curOffset.left ) + curLeft;
		}

		if ( "using" in options ) {
			options.using.call( elem, props );

		} else {
			curElem.css( props );
		}
	}
};

jQuery.fn.extend({
	offset: function( options ) {
		if ( arguments.length ) {
			return options === undefined ?
				this :
				this.each(function( i ) {
					jQuery.offset.setOffset( this, options, i );
				});
		}

		var docElem, win,
			elem = this[ 0 ],
			box = { top: 0, left: 0 },
			doc = elem && elem.ownerDocument;

		if ( !doc ) {
			return;
		}

		docElem = doc.documentElement;

		// Make sure it's not a disconnected DOM node
		if ( !jQuery.contains( docElem, elem ) ) {
			return box;
		}

		// Support: BlackBerry 5, iOS 3 (original iPhone)
		// If we don't have gBCR, just use 0,0 rather than error
		if ( typeof elem.getBoundingClientRect !== strundefined ) {
			box = elem.getBoundingClientRect();
		}
		win = getWindow( doc );
		return {
			top: box.top + win.pageYOffset - docElem.clientTop,
			left: box.left + win.pageXOffset - docElem.clientLeft
		};
	},

	position: function() {
		if ( !this[ 0 ] ) {
			return;
		}

		var offsetParent, offset,
			elem = this[ 0 ],
			parentOffset = { top: 0, left: 0 };

		// Fixed elements are offset from window (parentOffset = {top:0, left: 0}, because it is its only offset parent
		if ( jQuery.css( elem, "position" ) === "fixed" ) {
			// Assume getBoundingClientRect is there when computed position is fixed
			offset = elem.getBoundingClientRect();

		} else {
			// Get *real* offsetParent
			offsetParent = this.offsetParent();

			// Get correct offsets
			offset = this.offset();
			if ( !jQuery.nodeName( offsetParent[ 0 ], "html" ) ) {
				parentOffset = offsetParent.offset();
			}

			// Add offsetParent borders
			parentOffset.top += jQuery.css( offsetParent[ 0 ], "borderTopWidth", true );
			parentOffset.left += jQuery.css( offsetParent[ 0 ], "borderLeftWidth", true );
		}

		// Subtract parent offsets and element margins
		return {
			top: offset.top - parentOffset.top - jQuery.css( elem, "marginTop", true ),
			left: offset.left - parentOffset.left - jQuery.css( elem, "marginLeft", true )
		};
	},

	offsetParent: function() {
		return this.map(function() {
			var offsetParent = this.offsetParent || docElem;

			while ( offsetParent && ( !jQuery.nodeName( offsetParent, "html" ) && jQuery.css( offsetParent, "position" ) === "static" ) ) {
				offsetParent = offsetParent.offsetParent;
			}

			return offsetParent || docElem;
		});
	}
});

// Create scrollLeft and scrollTop methods
jQuery.each( { scrollLeft: "pageXOffset", scrollTop: "pageYOffset" }, function( method, prop ) {
	var top = "pageYOffset" === prop;

	jQuery.fn[ method ] = function( val ) {
		return access( this, function( elem, method, val ) {
			var win = getWindow( elem );

			if ( val === undefined ) {
				return win ? win[ prop ] : elem[ method ];
			}

			if ( win ) {
				win.scrollTo(
					!top ? val : window.pageXOffset,
					top ? val : window.pageYOffset
				);

			} else {
				elem[ method ] = val;
			}
		}, method, val, arguments.length, null );
	};
});

// Support: Safari<7+, Chrome<37+
// Add the top/left cssHooks using jQuery.fn.position
// Webkit bug: https://bugs.webkit.org/show_bug.cgi?id=29084
// Blink bug: https://code.google.com/p/chromium/issues/detail?id=229280
// getComputedStyle returns percent when specified for top/left/bottom/right;
// rather than make the css module depend on the offset module, just check for it here
jQuery.each( [ "top", "left" ], function( i, prop ) {
	jQuery.cssHooks[ prop ] = addGetHookIf( support.pixelPosition,
		function( elem, computed ) {
			if ( computed ) {
				computed = curCSS( elem, prop );
				// If curCSS returns percentage, fallback to offset
				return rnumnonpx.test( computed ) ?
					jQuery( elem ).position()[ prop ] + "px" :
					computed;
			}
		}
	);
});


// Create innerHeight, innerWidth, height, width, outerHeight and outerWidth methods
jQuery.each( { Height: "height", Width: "width" }, function( name, type ) {
	jQuery.each( { padding: "inner" + name, content: type, "": "outer" + name }, function( defaultExtra, funcName ) {
		// Margin is only for outerHeight, outerWidth
		jQuery.fn[ funcName ] = function( margin, value ) {
			var chainable = arguments.length && ( defaultExtra || typeof margin !== "boolean" ),
				extra = defaultExtra || ( margin === true || value === true ? "margin" : "border" );

			return access( this, function( elem, type, value ) {
				var doc;

				if ( jQuery.isWindow( elem ) ) {
					// As of 5/8/2012 this will yield incorrect results for Mobile Safari, but there
					// isn't a whole lot we can do. See pull request at this URL for discussion:
					// https://github.com/jquery/jquery/pull/764
					return elem.document.documentElement[ "client" + name ];
				}

				// Get document width or height
				if ( elem.nodeType === 9 ) {
					doc = elem.documentElement;

					// Either scroll[Width/Height] or offset[Width/Height] or client[Width/Height],
					// whichever is greatest
					return Math.max(
						elem.body[ "scroll" + name ], doc[ "scroll" + name ],
						elem.body[ "offset" + name ], doc[ "offset" + name ],
						doc[ "client" + name ]
					);
				}

				return value === undefined ?
					// Get width or height on the element, requesting but not forcing parseFloat
					jQuery.css( elem, type, extra ) :

					// Set width or height on the element
					jQuery.style( elem, type, value, extra );
			}, type, chainable ? margin : undefined, chainable, null );
		};
	});
});


// The number of elements contained in the matched element set
jQuery.fn.size = function() {
	return this.length;
};

jQuery.fn.andSelf = jQuery.fn.addBack;




// Register as a named AMD module, since jQuery can be concatenated with other
// files that may use define, but not via a proper concatenation script that
// understands anonymous AMD modules. A named AMD is safest and most robust
// way to register. Lowercase jquery is used because AMD module names are
// derived from file names, and jQuery is normally delivered in a lowercase
// file name. Do this after creating the global so that if an AMD module wants
// to call noConflict to hide this version of jQuery, it will work.

// Note that for maximum portability, libraries that are not jQuery should
// declare themselves as anonymous modules, and avoid setting a global if an
// AMD loader is present. jQuery is a special case. For more information, see
// https://github.com/jrburke/requirejs/wiki/Updating-existing-libraries#wiki-anon

if ( typeof define === "function" && define.amd ) {
	define( "jquery", [], function() {
		return jQuery;
	});
}




var
	// Map over jQuery in case of overwrite
	_jQuery = window.jQuery,

	// Map over the $ in case of overwrite
	_$ = window.$;

jQuery.noConflict = function( deep ) {
	if ( window.$ === jQuery ) {
		window.$ = _$;
	}

	if ( deep && window.jQuery === jQuery ) {
		window.jQuery = _jQuery;
	}

	return jQuery;
};

// Expose jQuery and $ identifiers, even in AMD
// (#7102#comment:10, https://github.com/jquery/jquery/pull/557)
// and CommonJS for browser emulators (#13566)
if ( typeof noGlobal === strundefined ) {
	window.jQuery = window.$ = jQuery;
}




return jQuery;

}));

/*
 * jQuery appear plugin
 *
 * Copyright (c) 2012 Andrey Sidorov
 * licensed under MIT license.
 *
 * https://github.com/morr/jquery.appear/
 *
 * Version: 0.3.6
 */
(function($) {
  var selectors = [];

  var check_binded = false;
  var check_lock = false;
  var defaults = {
    interval: 250,
    force_process: false
  };
  var $window = $(window);

  var $prior_appeared = [];

  function process() {
    check_lock = false;
    for (var index = 0, selectorsLength = selectors.length; index < selectorsLength; index++) {
      var $appeared = $(selectors[index]).filter(function() {
        return $(this).is(':appeared');
      });

      $appeared.trigger('appear', [$appeared]);

      if ($prior_appeared[index]) {
        var $disappeared = $prior_appeared[index].not($appeared);
        $disappeared.trigger('disappear', [$disappeared]);
      }
      $prior_appeared[index] = $appeared;
    }
  };

  function add_selector(selector) {
    selectors.push(selector);
    $prior_appeared.push();
  }

  // "appeared" custom filter
  $.expr[':']['appeared'] = function(element) {
    var $element = $(element);
    if (!$element.is(':visible')) {
      return false;
    }

    var window_left = $window.scrollLeft();
    var window_top = $window.scrollTop();
    var offset = $element.offset();
    var left = offset.left;
    var top = offset.top;

    if (top + $element.height() >= window_top &&
        top - ($element.data('appear-top-offset') || 0) <= window_top + $window.height() &&
        left + $element.width() >= window_left &&
        left - ($element.data('appear-left-offset') || 0) <= window_left + $window.width()) {
      return true;
    } else {
      return false;
    }
  };

  $.fn.extend({
    // watching for element's appearance in browser viewport
    appear: function(options) {
      var opts = $.extend({}, defaults, options || {});
      var selector = this.selector || this;
      if (!check_binded) {
        var on_check = function() {
          if (check_lock) {
            return;
          }
          check_lock = true;

          setTimeout(process, opts.interval);
        };

        $(window).scroll(on_check).resize(on_check);
        check_binded = true;
      }

      if (opts.force_process) {
        setTimeout(process, opts.interval);
      }
      add_selector(selector);
      return $(selector);
    }
  });

  $.extend({
    // force elements's appearance check
    force_appear: function() {
      if (check_binded) {
        process();
        return true;
      }
      return false;
    }
  });
})(function() {
  if (typeof module !== 'undefined') {
    // Node
    return require('jquery');
  } else {
    return jQuery;
  }
}());

/*!
 * jQuery Smooth Scroll - v1.5.7 - 2015-12-16
 * https://github.com/kswedberg/jquery-smooth-scroll
 * Copyright (c) 2015 Karl Swedberg
 * Licensed MIT (https://github.com/kswedberg/jquery-smooth-scroll/blob/master/LICENSE-MIT)
 */

(function (factory) {
  if (typeof define === 'function' && define.amd) {
    // AMD. Register as an anonymous module.
    define(['jquery'], factory);
  } else if (typeof module === 'object' && module.exports) {
    // CommonJS
    factory(require('jquery'));
  } else {
    // Browser globals
    factory(jQuery);
  }
}(function ($) {

  var version = '1.5.7',
      optionOverrides = {},
      defaults = {
        exclude: [],
        excludeWithin:[],
        offset: 0,

        // one of 'top' or 'left'
        direction: 'top',

        // jQuery set of elements you wish to scroll (for $.smoothScroll).
        //  if null (default), $('html, body').firstScrollable() is used.
        scrollElement: null,

        // only use if you want to override default behavior
        scrollTarget: null,

        // fn(opts) function to be called before scrolling occurs.
        // `this` is the element(s) being scrolled
        beforeScroll: function() {},

        // fn(opts) function to be called after scrolling occurs.
        // `this` is the triggering element
        afterScroll: function() {},
        easing: 'swing',
        speed: 400,

        // coefficient for "auto" speed
        autoCoefficient: 2,

        // $.fn.smoothScroll only: whether to prevent the default click action
        preventDefault: true
      },

      getScrollable = function(opts) {
        var scrollable = [],
            scrolled = false,
            dir = opts.dir && opts.dir === 'left' ? 'scrollLeft' : 'scrollTop';

        this.each(function() {
          var el = $(this);

          if (this === document || this === window) {
            return;
          }

          if ( document.scrollingElement && (this === document.documentElement || this === document.body) ) {
            scrollable.push(document.scrollingElement);

            return false;
          }

          if ( el[dir]() > 0 ) {
            scrollable.push(this);
          } else {
            // if scroll(Top|Left) === 0, nudge the element 1px and see if it moves
            el[dir](1);
            scrolled = el[dir]() > 0;
            if ( scrolled ) {
              scrollable.push(this);
            }
            // then put it back, of course
            el[dir](0);
          }
        });

        // If no scrollable elements, fall back to <body>,
        // if it's in the jQuery collection
        // (doing this because Safari sets scrollTop async,
        // so can't set it to 1 and immediately get the value.)
        if (!scrollable.length) {
          this.each(function() {
            if (this.nodeName === 'BODY') {
              scrollable = [this];
            }
          });
        }

        // Use the first scrollable element if we're calling firstScrollable()
        if ( opts.el === 'first' && scrollable.length > 1 ) {
          scrollable = [ scrollable[0] ];
        }

        return scrollable;
      };

  $.fn.extend({
    scrollable: function(dir) {
      var scrl = getScrollable.call(this, {dir: dir});
      return this.pushStack(scrl);
    },
    firstScrollable: function(dir) {
      var scrl = getScrollable.call(this, {el: 'first', dir: dir});
      return this.pushStack(scrl);
    },

    smoothScroll: function(options, extra) {
      options = options || {};

      if ( options === 'options' ) {
        if ( !extra ) {
          return this.first().data('ssOpts');
        }
        return this.each(function() {
          var $this = $(this),
              opts = $.extend($this.data('ssOpts') || {}, extra);

          $(this).data('ssOpts', opts);
        });
      }

      var opts = $.extend({}, $.fn.smoothScroll.defaults, options),
          locationPath = $.smoothScroll.filterPath(location.pathname);

      this
      .unbind('click.smoothscroll')
      .bind('click.smoothscroll', function(event) {
        var link = this,
            $link = $(this),
            thisOpts = $.extend({}, opts, $link.data('ssOpts') || {}),
            exclude = opts.exclude,
            excludeWithin = thisOpts.excludeWithin,
            elCounter = 0, ewlCounter = 0,
            include = true,
            clickOpts = {},
            hostMatch = ((location.hostname === link.hostname) || !link.hostname),
            pathMatch = thisOpts.scrollTarget || ( $.smoothScroll.filterPath(link.pathname) === locationPath ),
            thisHash = escapeSelector(link.hash);

        if ( !thisOpts.scrollTarget && (!hostMatch || !pathMatch || !thisHash) ) {
          include = false;
        } else {
          while (include && elCounter < exclude.length) {
            if ($link.is(escapeSelector(exclude[elCounter++]))) {
              include = false;
            }
          }
          while ( include && ewlCounter < excludeWithin.length ) {
            if ($link.closest(excludeWithin[ewlCounter++]).length) {
              include = false;
            }
          }
        }

        if ( include ) {

          if ( thisOpts.preventDefault ) {
            event.preventDefault();
          }

          $.extend( clickOpts, thisOpts, {
            scrollTarget: thisOpts.scrollTarget || thisHash,
            link: link
          });

          $.smoothScroll( clickOpts );
        }
      });

      return this;
    }
  });

  $.smoothScroll = function(options, px) {
    if ( options === 'options' && typeof px === 'object' ) {
      return $.extend(optionOverrides, px);
    }
    var opts, $scroller, scrollTargetOffset, speed, delta,
        scrollerOffset = 0,
        offPos = 'offset',
        scrollDir = 'scrollTop',
        aniProps = {},
        aniOpts = {};

    if (typeof options === 'number') {
      opts = $.extend({link: null}, $.fn.smoothScroll.defaults, optionOverrides);
      scrollTargetOffset = options;
    } else {
      opts = $.extend({link: null}, $.fn.smoothScroll.defaults, options || {}, optionOverrides);
      if (opts.scrollElement) {
        offPos = 'position';
        if (opts.scrollElement.css('position') === 'static') {
          opts.scrollElement.css('position', 'relative');
        }
      }
    }

    scrollDir = opts.direction === 'left' ? 'scrollLeft' : scrollDir;

    if ( opts.scrollElement ) {
      $scroller = opts.scrollElement;
      if ( !(/^(?:HTML|BODY)$/).test($scroller[0].nodeName) ) {
        scrollerOffset = $scroller[scrollDir]();
      }
    } else {
      $scroller = $('html, body').firstScrollable(opts.direction);
    }

    // beforeScroll callback function must fire before calculating offset
    opts.beforeScroll.call($scroller, opts);

    scrollTargetOffset = (typeof options === 'number') ? options :
                          px ||
                          ( $(opts.scrollTarget)[offPos]() &&
                          $(opts.scrollTarget)[offPos]()[opts.direction] ) ||
                          0;

    aniProps[scrollDir] = scrollTargetOffset + scrollerOffset + opts.offset;
    speed = opts.speed;

    // automatically calculate the speed of the scroll based on distance / coefficient
    if (speed === 'auto') {

      // $scroller[scrollDir]() is position before scroll, aniProps[scrollDir] is position after
      // When delta is greater, speed will be greater.
      delta = Math.abs(aniProps[scrollDir] - $scroller[scrollDir]());

      // Divide the delta by the coefficient
      speed = delta / opts.autoCoefficient;
    }

    aniOpts = {
      duration: speed,
      easing: opts.easing,
      complete: function() {
        opts.afterScroll.call(opts.link, opts);
      }
    };

    if (opts.step) {
      aniOpts.step = opts.step;
    }

    if ($scroller.length) {
      $scroller.stop().animate(aniProps, aniOpts);
    } else {
      opts.afterScroll.call(opts.link, opts);
    }
  };

  $.smoothScroll.version = version;
  $.smoothScroll.filterPath = function(string) {
    string = string || '';
    return string
      .replace(/^\//,'')
      .replace(/(?:index|default).[a-zA-Z]{3,4}$/,'')
      .replace(/\/$/,'');
  };

  // default options
  $.fn.smoothScroll.defaults = defaults;

  function escapeSelector (str) {
    return str.replace(/(:|\.|\/)/g,'\\$1');
  }

}));


/*!
 * jQuery Transit - CSS3 transitions and transformations
 * (c) 2011-2014 Rico Sta. Cruz
 * MIT Licensed.
 *
 * http://ricostacruz.com/jquery.transit
 * http://github.com/rstacruz/jquery.transit
 */

/* jshint expr: true */

;(function (root, factory) {

  if (typeof define === 'function' && define.amd) {
    define(['jquery'], factory);
  } else if (typeof exports === 'object') {
    module.exports = factory(require('jquery'));
  } else {
    factory(root.jQuery);
  }

}(this, function($) {

  $.transit = {
    version: "0.9.12",

    // Map of $.css() keys to values for 'transitionProperty'.
    // See https://developer.mozilla.org/en/CSS/CSS_transitions#Properties_that_can_be_animated
    propertyMap: {
      marginLeft    : 'margin',
      marginRight   : 'margin',
      marginBottom  : 'margin',
      marginTop     : 'margin',
      paddingLeft   : 'padding',
      paddingRight  : 'padding',
      paddingBottom : 'padding',
      paddingTop    : 'padding'
    },

    // Will simply transition "instantly" if false
    enabled: true,

    // Set this to false if you don't want to use the transition end property.
    useTransitionEnd: false
  };

  var div = document.createElement('div');
  var support = {};

  // Helper function to get the proper vendor property name.
  // (`transition` => `WebkitTransition`)
  function getVendorPropertyName(prop) {
    // Handle unprefixed versions (FF16+, for example)
    if (prop in div.style) return prop;

    var prefixes = ['Moz', 'Webkit', 'O', 'ms'];
    var prop_ = prop.charAt(0).toUpperCase() + prop.substr(1);

    for (var i=0; i<prefixes.length; ++i) {
      var vendorProp = prefixes[i] + prop_;
      if (vendorProp in div.style) { return vendorProp; }
    }
  }

  // Helper function to check if transform3D is supported.
  // Should return true for Webkits and Firefox 10+.
  function checkTransform3dSupport() {
    div.style[support.transform] = '';
    div.style[support.transform] = 'rotateY(90deg)';
    return div.style[support.transform] !== '';
  }

  var isChrome = navigator.userAgent.toLowerCase().indexOf('chrome') > -1;

  // Check for the browser's transitions support.
  support.transition      = getVendorPropertyName('transition');
  support.transitionDelay = getVendorPropertyName('transitionDelay');
  support.transform       = getVendorPropertyName('transform');
  support.transformOrigin = getVendorPropertyName('transformOrigin');
  support.filter          = getVendorPropertyName('Filter');
  support.transform3d     = checkTransform3dSupport();

  var eventNames = {
    'transition':       'transitionend',
    'MozTransition':    'transitionend',
    'OTransition':      'oTransitionEnd',
    'WebkitTransition': 'webkitTransitionEnd',
    'msTransition':     'MSTransitionEnd'
  };

  // Detect the 'transitionend' event needed.
  var transitionEnd = support.transitionEnd = eventNames[support.transition] || null;

  // Populate jQuery's `$.support` with the vendor prefixes we know.
  // As per [jQuery's cssHooks documentation](http://api.jquery.com/jQuery.cssHooks/),
  // we set $.support.transition to a string of the actual property name used.
  for (var key in support) {
    if (support.hasOwnProperty(key) && typeof $.support[key] === 'undefined') {
      $.support[key] = support[key];
    }
  }

  // Avoid memory leak in IE.
  div = null;

  // ## $.cssEase
  // List of easing aliases that you can use with `$.fn.transition`.
  $.cssEase = {
    '_default':       'ease',
    'in':             'ease-in',
    'out':            'ease-out',
    'in-out':         'ease-in-out',
    'snap':           'cubic-bezier(0,1,.5,1)',
    // Penner equations
    'easeInCubic':    'cubic-bezier(.550,.055,.675,.190)',
    'easeOutCubic':   'cubic-bezier(.215,.61,.355,1)',
    'easeInOutCubic': 'cubic-bezier(.645,.045,.355,1)',
    'easeInCirc':     'cubic-bezier(.6,.04,.98,.335)',
    'easeOutCirc':    'cubic-bezier(.075,.82,.165,1)',
    'easeInOutCirc':  'cubic-bezier(.785,.135,.15,.86)',
    'easeInExpo':     'cubic-bezier(.95,.05,.795,.035)',
    'easeOutExpo':    'cubic-bezier(.19,1,.22,1)',
    'easeInOutExpo':  'cubic-bezier(1,0,0,1)',
    'easeInQuad':     'cubic-bezier(.55,.085,.68,.53)',
    'easeOutQuad':    'cubic-bezier(.25,.46,.45,.94)',
    'easeInOutQuad':  'cubic-bezier(.455,.03,.515,.955)',
    'easeInQuart':    'cubic-bezier(.895,.03,.685,.22)',
    'easeOutQuart':   'cubic-bezier(.165,.84,.44,1)',
    'easeInOutQuart': 'cubic-bezier(.77,0,.175,1)',
    'easeInQuint':    'cubic-bezier(.755,.05,.855,.06)',
    'easeOutQuint':   'cubic-bezier(.23,1,.32,1)',
    'easeInOutQuint': 'cubic-bezier(.86,0,.07,1)',
    'easeInSine':     'cubic-bezier(.47,0,.745,.715)',
    'easeOutSine':    'cubic-bezier(.39,.575,.565,1)',
    'easeInOutSine':  'cubic-bezier(.445,.05,.55,.95)',
    'easeInBack':     'cubic-bezier(.6,-.28,.735,.045)',
    'easeOutBack':    'cubic-bezier(.175, .885,.32,1.275)',
    'easeInOutBack':  'cubic-bezier(.68,-.55,.265,1.55)'
  };

  // ## 'transform' CSS hook
  // Allows you to use the `transform` property in CSS.
  //
  //     $("#hello").css({ transform: "rotate(90deg)" });
  //
  //     $("#hello").css('transform');
  //     //=> { rotate: '90deg' }
  //
  $.cssHooks['transit:transform'] = {
    // The getter returns a `Transform` object.
    get: function(elem) {
      return $(elem).data('transform') || new Transform();
    },

    // The setter accepts a `Transform` object or a string.
    set: function(elem, v) {
      var value = v;

      if (!(value instanceof Transform)) {
        value = new Transform(value);
      }

      // We've seen the 3D version of Scale() not work in Chrome when the
      // element being scaled extends outside of the viewport.  Thus, we're
      // forcing Chrome to not use the 3d transforms as well.  Not sure if
      // translate is affectede, but not risking it.  Detection code from
      // http://davidwalsh.name/detecting-google-chrome-javascript
      if (support.transform === 'WebkitTransform' && !isChrome) {
        elem.style[support.transform] = value.toString(true);
      } else {
        elem.style[support.transform] = value.toString();
      }

      $(elem).data('transform', value);
    }
  };

  // Add a CSS hook for `.css({ transform: '...' })`.
  // In jQuery 1.8+, this will intentionally override the default `transform`
  // CSS hook so it'll play well with Transit. (see issue #62)
  $.cssHooks.transform = {
    set: $.cssHooks['transit:transform'].set
  };

  // ## 'filter' CSS hook
  // Allows you to use the `filter` property in CSS.
  //
  //     $("#hello").css({ filter: 'blur(10px)' });
  //
  $.cssHooks.filter = {
    get: function(elem) {
      return elem.style[support.filter];
    },
    set: function(elem, value) {
      elem.style[support.filter] = value;
    }
  };

  // jQuery 1.8+ supports prefix-free transitions, so these polyfills will not
  // be necessary.
  if ($.fn.jquery < "1.8") {
    // ## 'transformOrigin' CSS hook
    // Allows the use for `transformOrigin` to define where scaling and rotation
    // is pivoted.
    //
    //     $("#hello").css({ transformOrigin: '0 0' });
    //
    $.cssHooks.transformOrigin = {
      get: function(elem) {
        return elem.style[support.transformOrigin];
      },
      set: function(elem, value) {
        elem.style[support.transformOrigin] = value;
      }
    };

    // ## 'transition' CSS hook
    // Allows you to use the `transition` property in CSS.
    //
    //     $("#hello").css({ transition: 'all 0 ease 0' });
    //
    $.cssHooks.transition = {
      get: function(elem) {
        return elem.style[support.transition];
      },
      set: function(elem, value) {
        elem.style[support.transition] = value;
      }
    };
  }

  // ## Other CSS hooks
  // Allows you to rotate, scale and translate.
  registerCssHook('scale');
  registerCssHook('scaleX');
  registerCssHook('scaleY');
  registerCssHook('translate');
  registerCssHook('rotate');
  registerCssHook('rotateX');
  registerCssHook('rotateY');
  registerCssHook('rotate3d');
  registerCssHook('perspective');
  registerCssHook('skewX');
  registerCssHook('skewY');
  registerCssHook('x', true);
  registerCssHook('y', true);

  // ## Transform class
  // This is the main class of a transformation property that powers
  // `$.fn.css({ transform: '...' })`.
  //
  // This is, in essence, a dictionary object with key/values as `-transform`
  // properties.
  //
  //     var t = new Transform("rotate(90) scale(4)");
  //
  //     t.rotate             //=> "90deg"
  //     t.scale              //=> "4,4"
  //
  // Setters are accounted for.
  //
  //     t.set('rotate', 4)
  //     t.rotate             //=> "4deg"
  //
  // Convert it to a CSS string using the `toString()` and `toString(true)` (for WebKit)
  // functions.
  //
  //     t.toString()         //=> "rotate(90deg) scale(4,4)"
  //     t.toString(true)     //=> "rotate(90deg) scale3d(4,4,0)" (WebKit version)
  //
  function Transform(str) {
    if (typeof str === 'string') { this.parse(str); }
    return this;
  }

  Transform.prototype = {
    // ### setFromString()
    // Sets a property from a string.
    //
    //     t.setFromString('scale', '2,4');
    //     // Same as set('scale', '2', '4');
    //
    setFromString: function(prop, val) {
      var args =
        (typeof val === 'string')  ? val.split(',') :
        (val.constructor === Array) ? val :
        [ val ];

      args.unshift(prop);

      Transform.prototype.set.apply(this, args);
    },

    // ### set()
    // Sets a property.
    //
    //     t.set('scale', 2, 4);
    //
    set: function(prop) {
      var args = Array.prototype.slice.apply(arguments, [1]);
      if (this.setter[prop]) {
        this.setter[prop].apply(this, args);
      } else {
        this[prop] = args.join(',');
      }
    },

    get: function(prop) {
      if (this.getter[prop]) {
        return this.getter[prop].apply(this);
      } else {
        return this[prop] || 0;
      }
    },

    setter: {
      // ### rotate
      //
      //     .css({ rotate: 30 })
      //     .css({ rotate: "30" })
      //     .css({ rotate: "30deg" })
      //     .css({ rotate: "30deg" })
      //
      rotate: function(theta) {
        this.rotate = unit(theta, 'deg');
      },

      rotateX: function(theta) {
        this.rotateX = unit(theta, 'deg');
      },

      rotateY: function(theta) {
        this.rotateY = unit(theta, 'deg');
      },

      // ### scale
      //
      //     .css({ scale: 9 })      //=> "scale(9,9)"
      //     .css({ scale: '3,2' })  //=> "scale(3,2)"
      //
      scale: function(x, y) {
        if (y === undefined) { y = x; }
        this.scale = x + "," + y;
      },

      // ### skewX + skewY
      skewX: function(x) {
        this.skewX = unit(x, 'deg');
      },

      skewY: function(y) {
        this.skewY = unit(y, 'deg');
      },

      // ### perspectvie
      perspective: function(dist) {
        this.perspective = unit(dist, 'px');
      },

      // ### x / y
      // Translations. Notice how this keeps the other value.
      //
      //     .css({ x: 4 })       //=> "translate(4px, 0)"
      //     .css({ y: 10 })      //=> "translate(4px, 10px)"
      //
      x: function(x) {
        this.set('translate', x, null);
      },

      y: function(y) {
        this.set('translate', null, y);
      },

      // ### translate
      // Notice how this keeps the other value.
      //
      //     .css({ translate: '2, 5' })    //=> "translate(2px, 5px)"
      //
      translate: function(x, y) {
        if (this._translateX === undefined) { this._translateX = 0; }
        if (this._translateY === undefined) { this._translateY = 0; }

        if (x !== null && x !== undefined) { this._translateX = unit(x, 'px'); }
        if (y !== null && y !== undefined) { this._translateY = unit(y, 'px'); }

        this.translate = this._translateX + "," + this._translateY;
      }
    },

    getter: {
      x: function() {
        return this._translateX || 0;
      },

      y: function() {
        return this._translateY || 0;
      },

      scale: function() {
        var s = (this.scale || "1,1").split(',');
        if (s[0]) { s[0] = parseFloat(s[0]); }
        if (s[1]) { s[1] = parseFloat(s[1]); }

        // "2.5,2.5" => 2.5
        // "2.5,1" => [2.5,1]
        return (s[0] === s[1]) ? s[0] : s;
      },

      rotate3d: function() {
        var s = (this.rotate3d || "0,0,0,0deg").split(',');
        for (var i=0; i<=3; ++i) {
          if (s[i]) { s[i] = parseFloat(s[i]); }
        }
        if (s[3]) { s[3] = unit(s[3], 'deg'); }

        return s;
      }
    },

    // ### parse()
    // Parses from a string. Called on constructor.
    parse: function(str) {
      var self = this;
      str.replace(/([a-zA-Z0-9]+)\((.*?)\)/g, function(x, prop, val) {
        self.setFromString(prop, val);
      });
    },

    // ### toString()
    // Converts to a `transition` CSS property string. If `use3d` is given,
    // it converts to a `-webkit-transition` CSS property string instead.
    toString: function(use3d) {
      var re = [];

      for (var i in this) {
        if (this.hasOwnProperty(i)) {
          // Don't use 3D transformations if the browser can't support it.
          if ((!support.transform3d) && (
            (i === 'rotateX') ||
            (i === 'rotateY') ||
            (i === 'perspective') ||
            (i === 'transformOrigin'))) { continue; }

          if (i[0] !== '_') {
            if (use3d && (i === 'scale')) {
              re.push(i + "3d(" + this[i] + ",1)");
            } else if (use3d && (i === 'translate')) {
              re.push(i + "3d(" + this[i] + ",0)");
            } else {
              re.push(i + "(" + this[i] + ")");
            }
          }
        }
      }

      return re.join(" ");
    }
  };

  function callOrQueue(self, queue, fn) {
    if (queue === true) {
      self.queue(fn);
    } else if (queue) {
      self.queue(queue, fn);
    } else {
      self.each(function () {
                fn.call(this);
            });
    }
  }

  // ### getProperties(dict)
  // Returns properties (for `transition-property`) for dictionary `props`. The
  // value of `props` is what you would expect in `$.css(...)`.
  function getProperties(props) {
    var re = [];

    $.each(props, function(key) {
      key = $.camelCase(key); // Convert "text-align" => "textAlign"
      key = $.transit.propertyMap[key] || $.cssProps[key] || key;
      key = uncamel(key); // Convert back to dasherized

      // Get vendor specify propertie
      if (support[key])
        key = uncamel(support[key]);

      if ($.inArray(key, re) === -1) { re.push(key); }
    });

    return re;
  }

  // ### getTransition()
  // Returns the transition string to be used for the `transition` CSS property.
  //
  // Example:
  //
  //     getTransition({ opacity: 1, rotate: 30 }, 500, 'ease');
  //     //=> 'opacity 500ms ease, -webkit-transform 500ms ease'
  //
  function getTransition(properties, duration, easing, delay) {
    // Get the CSS properties needed.
    var props = getProperties(properties);

    // Account for aliases (`in` => `ease-in`).
    if ($.cssEase[easing]) { easing = $.cssEase[easing]; }

    // Build the duration/easing/delay attributes for it.
    var attribs = '' + toMS(duration) + ' ' + easing;
    if (parseInt(delay, 10) > 0) { attribs += ' ' + toMS(delay); }

    // For more properties, add them this way:
    // "margin 200ms ease, padding 200ms ease, ..."
    var transitions = [];
    $.each(props, function(i, name) {
      transitions.push(name + ' ' + attribs);
    });

    return transitions.join(', ');
  }

  // ## $.fn.transition
  // Works like $.fn.animate(), but uses CSS transitions.
  //
  //     $("...").transition({ opacity: 0.1, scale: 0.3 });
  //
  //     // Specific duration
  //     $("...").transition({ opacity: 0.1, scale: 0.3 }, 500);
  //
  //     // With duration and easing
  //     $("...").transition({ opacity: 0.1, scale: 0.3 }, 500, 'in');
  //
  //     // With callback
  //     $("...").transition({ opacity: 0.1, scale: 0.3 }, function() { ... });
  //
  //     // With everything
  //     $("...").transition({ opacity: 0.1, scale: 0.3 }, 500, 'in', function() { ... });
  //
  //     // Alternate syntax
  //     $("...").transition({
  //       opacity: 0.1,
  //       duration: 200,
  //       delay: 40,
  //       easing: 'in',
  //       complete: function() { /* ... */ }
  //      });
  //
  $.fn.transition = $.fn.transit = function(properties, duration, easing, callback) {
    var self  = this;
    var delay = 0;
    var queue = true;

    var theseProperties = $.extend(true, {}, properties);

    // Account for `.transition(properties, callback)`.
    if (typeof duration === 'function') {
      callback = duration;
      duration = undefined;
    }

    // Account for `.transition(properties, options)`.
    if (typeof duration === 'object') {
      easing = duration.easing;
      delay = duration.delay || 0;
      queue = typeof duration.queue === "undefined" ? true : duration.queue;
      callback = duration.complete;
      duration = duration.duration;
    }

    // Account for `.transition(properties, duration, callback)`.
    if (typeof easing === 'function') {
      callback = easing;
      easing = undefined;
    }

    // Alternate syntax.
    if (typeof theseProperties.easing !== 'undefined') {
      easing = theseProperties.easing;
      delete theseProperties.easing;
    }

    if (typeof theseProperties.duration !== 'undefined') {
      duration = theseProperties.duration;
      delete theseProperties.duration;
    }

    if (typeof theseProperties.complete !== 'undefined') {
      callback = theseProperties.complete;
      delete theseProperties.complete;
    }

    if (typeof theseProperties.queue !== 'undefined') {
      queue = theseProperties.queue;
      delete theseProperties.queue;
    }

    if (typeof theseProperties.delay !== 'undefined') {
      delay = theseProperties.delay;
      delete theseProperties.delay;
    }

    // Set defaults. (`400` duration, `ease` easing)
    if (typeof duration === 'undefined') { duration = $.fx.speeds._default; }
    if (typeof easing === 'undefined')   { easing = $.cssEase._default; }

    duration = toMS(duration);

    // Build the `transition` property.
    var transitionValue = getTransition(theseProperties, duration, easing, delay);

    // Compute delay until callback.
    // If this becomes 0, don't bother setting the transition property.
    var work = $.transit.enabled && support.transition;
    var i = work ? (parseInt(duration, 10) + parseInt(delay, 10)) : 0;

    // If there's nothing to do...
    if (i === 0) {
      var fn = function(next) {
        self.css(theseProperties);
        if (callback) { callback.apply(self); }
        if (next) { next(); }
      };

      callOrQueue(self, queue, fn);
      return self;
    }

    // Save the old transitions of each element so we can restore it later.
    var oldTransitions = {};

    var run = function(nextCall) {
      var bound = false;

      // Prepare the callback.
      var cb = function() {
        if (bound) { self.unbind(transitionEnd, cb); }

        if (i > 0) {
          self.each(function() {
            this.style[support.transition] = (oldTransitions[this] || null);
          });
        }

        if (typeof callback === 'function') { callback.apply(self); }
        if (typeof nextCall === 'function') { nextCall(); }
      };

      if ((i > 0) && (transitionEnd) && ($.transit.useTransitionEnd)) {
        // Use the 'transitionend' event if it's available.
        bound = true;
        self.bind(transitionEnd, cb);
      } else {
        // Fallback to timers if the 'transitionend' event isn't supported.
        window.setTimeout(cb, i);
      }

      // Apply transitions.
      self.each(function() {
        if (i > 0) {
          this.style[support.transition] = transitionValue;
        }
        $(this).css(theseProperties);
      });
    };

    // Defer running. This allows the browser to paint any pending CSS it hasn't
    // painted yet before doing the transitions.
    var deferredRun = function(next) {
        this.offsetWidth; // force a repaint
        run(next);
    };

    // Use jQuery's fx queue.
    callOrQueue(self, queue, deferredRun);

    // Chainability.
    return this;
  };

  function registerCssHook(prop, isPixels) {
    // For certain properties, the 'px' should not be implied.
    if (!isPixels) { $.cssNumber[prop] = true; }

    $.transit.propertyMap[prop] = support.transform;

    $.cssHooks[prop] = {
      get: function(elem) {
        var t = $(elem).css('transit:transform');
        return t.get(prop);
      },

      set: function(elem, value) {
        var t = $(elem).css('transit:transform');
        t.setFromString(prop, value);

        $(elem).css({ 'transit:transform': t });
      }
    };

  }

  // ### uncamel(str)
  // Converts a camelcase string to a dasherized string.
  // (`marginLeft` => `margin-left`)
  function uncamel(str) {
    return str.replace(/([A-Z])/g, function(letter) { return '-' + letter.toLowerCase(); });
  }

  // ### unit(number, unit)
  // Ensures that number `number` has a unit. If no unit is found, assume the
  // default is `unit`.
  //
  //     unit(2, 'px')          //=> "2px"
  //     unit("30deg", 'rad')   //=> "30deg"
  //
  function unit(i, units) {
    if ((typeof i === "string") && (!i.match(/^[\-0-9\.]+$/))) {
      return i;
    } else {
      return "" + i + units;
    }
  }

  // ### toMS(duration)
  // Converts given `duration` to a millisecond string.
  //
  // toMS('fast') => $.fx.speeds[i] => "200ms"
  // toMS('normal') //=> $.fx.speeds._default => "400ms"
  // toMS(10) //=> '10ms'
  // toMS('100ms') //=> '100ms'  
  //
  function toMS(duration) {
    var i = duration;

    // Allow string durations like 'fast' and 'slow', without overriding numeric values.
    if (typeof i === 'string' && (!i.match(/^[\-0-9\.]+/))) { i = $.fx.speeds[i] || $.fx.speeds._default; }

    return unit(i, 'ms');
  }

  // Export some functions for testable-ness.
  $.transit.getTransitionValue = getTransition;

  return $;
}));

/*
    jQuery Masked Input Plugin
    Copyright (c) 2007 - 2015 Josh Bush (digitalbush.com)
    Licensed under the MIT license (http://digitalbush.com/projects/masked-input-plugin/#license)
    Version: 1.4.1
*/
!function(factory) {
    "function" == typeof define && define.amd ? define([ "jquery" ], factory) : factory("object" == typeof exports ? require("jquery") : jQuery);
}(function($) {
    var caretTimeoutId, ua = navigator.userAgent, iPhone = /iphone/i.test(ua), chrome = /chrome/i.test(ua), android = /android/i.test(ua);
    $.mask = {
        definitions: {
            "9": "[0-9]",
            a: "[A-Za-z]",
            "*": "[A-Za-z0-9]"
        },
        autoclear: !0,
        dataName: "rawMaskFn",
        placeholder: "_"
    }, $.fn.extend({
        caret: function(begin, end) {
            var range;
            if (0 !== this.length && !this.is(":hidden")) return "number" == typeof begin ? (end = "number" == typeof end ? end : begin, 
            this.each(function() {
                this.setSelectionRange ? this.setSelectionRange(begin, end) : this.createTextRange && (range = this.createTextRange(), 
                range.collapse(!0), range.moveEnd("character", end), range.moveStart("character", begin), 
                range.select());
            })) : (this[0].setSelectionRange ? (begin = this[0].selectionStart, end = this[0].selectionEnd) : document.selection && document.selection.createRange && (range = document.selection.createRange(), 
            begin = 0 - range.duplicate().moveStart("character", -1e5), end = begin + range.text.length), 
            {
                begin: begin,
                end: end
            });
        },
        unmask: function() {
            return this.trigger("unmask");
        },
        mask: function(mask, settings) {
            var input, defs, tests, partialPosition, firstNonMaskPos, lastRequiredNonMaskPos, len, oldVal;
            if (!mask && this.length > 0) {
                input = $(this[0]);
                var fn = input.data($.mask.dataName);
                return fn ? fn() : void 0;
            }
            return settings = $.extend({
                autoclear: $.mask.autoclear,
                placeholder: $.mask.placeholder,
                completed: null
            }, settings), defs = $.mask.definitions, tests = [], partialPosition = len = mask.length, 
            firstNonMaskPos = null, $.each(mask.split(""), function(i, c) {
                "?" == c ? (len--, partialPosition = i) : defs[c] ? (tests.push(new RegExp(defs[c])), 
                null === firstNonMaskPos && (firstNonMaskPos = tests.length - 1), partialPosition > i && (lastRequiredNonMaskPos = tests.length - 1)) : tests.push(null);
            }), this.trigger("unmask").each(function() {
                function tryFireCompleted() {
                    if (settings.completed) {
                        for (var i = firstNonMaskPos; lastRequiredNonMaskPos >= i; i++) if (tests[i] && buffer[i] === getPlaceholder(i)) return;
                        settings.completed.call(input);
                    }
                }
                function getPlaceholder(i) {
                    return settings.placeholder.charAt(i < settings.placeholder.length ? i : 0);
                }
                function seekNext(pos) {
                    for (;++pos < len && !tests[pos]; ) ;
                    return pos;
                }
                function seekPrev(pos) {
                    for (;--pos >= 0 && !tests[pos]; ) ;
                    return pos;
                }
                function shiftL(begin, end) {
                    var i, j;
                    if (!(0 > begin)) {
                        for (i = begin, j = seekNext(end); len > i; i++) if (tests[i]) {
                            if (!(len > j && tests[i].test(buffer[j]))) break;
                            buffer[i] = buffer[j], buffer[j] = getPlaceholder(j), j = seekNext(j);
                        }
                        writeBuffer(), input.caret(Math.max(firstNonMaskPos, begin));
                    }
                }
                function shiftR(pos) {
                    var i, c, j, t;
                    for (i = pos, c = getPlaceholder(pos); len > i; i++) if (tests[i]) {
                        if (j = seekNext(i), t = buffer[i], buffer[i] = c, !(len > j && tests[j].test(t))) break;
                        c = t;
                    }
                }
                function androidInputEvent() {
                    var curVal = input.val(), pos = input.caret();
                    if (oldVal && oldVal.length && oldVal.length > curVal.length) {
                        for (checkVal(!0); pos.begin > 0 && !tests[pos.begin - 1]; ) pos.begin--;
                        if (0 === pos.begin) for (;pos.begin < firstNonMaskPos && !tests[pos.begin]; ) pos.begin++;
                        input.caret(pos.begin, pos.begin);
                    } else {
                        for (checkVal(!0); pos.begin < len && !tests[pos.begin]; ) pos.begin++;
                        input.caret(pos.begin, pos.begin);
                    }
                    tryFireCompleted();
                }
                function blurEvent() {
                    checkVal(), input.val() != focusText && input.change();
                }
                function keydownEvent(e) {
                    if (!input.prop("readonly")) {
                        var pos, begin, end, k = e.which || e.keyCode;
                        oldVal = input.val(), 8 === k || 46 === k || iPhone && 127 === k ? (pos = input.caret(), 
                        begin = pos.begin, end = pos.end, end - begin === 0 && (begin = 46 !== k ? seekPrev(begin) : end = seekNext(begin - 1), 
                        end = 46 === k ? seekNext(end) : end), clearBuffer(begin, end), shiftL(begin, end - 1), 
                        e.preventDefault()) : 13 === k ? blurEvent.call(this, e) : 27 === k && (input.val(focusText), 
                        input.caret(0, checkVal()), e.preventDefault());
                    }
                }
                function keypressEvent(e) {
                    if (!input.prop("readonly")) {
                        var p, c, next, k = e.which || e.keyCode, pos = input.caret();
                        if (!(e.ctrlKey || e.altKey || e.metaKey || 32 > k) && k && 13 !== k) {
                            if (pos.end - pos.begin !== 0 && (clearBuffer(pos.begin, pos.end), shiftL(pos.begin, pos.end - 1)), 
                            p = seekNext(pos.begin - 1), len > p && (c = String.fromCharCode(k), tests[p].test(c))) {
                                if (shiftR(p), buffer[p] = c, writeBuffer(), next = seekNext(p), android) {
                                    var proxy = function() {
                                        $.proxy($.fn.caret, input, next)();
                                    };
                                    setTimeout(proxy, 0);
                                } else input.caret(next);
                                pos.begin <= lastRequiredNonMaskPos && tryFireCompleted();
                            }
                            e.preventDefault();
                        }
                    }
                }
                function clearBuffer(start, end) {
                    var i;
                    for (i = start; end > i && len > i; i++) tests[i] && (buffer[i] = getPlaceholder(i));
                }
                function writeBuffer() {
                    input.val(buffer.join(""));
                }
                function checkVal(allow) {
                    var i, c, pos, test = input.val(), lastMatch = -1;
                    for (i = 0, pos = 0; len > i; i++) if (tests[i]) {
                        for (buffer[i] = getPlaceholder(i); pos++ < test.length; ) if (c = test.charAt(pos - 1), 
                        tests[i].test(c)) {
                            buffer[i] = c, lastMatch = i;
                            break;
                        }
                        if (pos > test.length) {
                            clearBuffer(i + 1, len);
                            break;
                        }
                    } else buffer[i] === test.charAt(pos) && pos++, partialPosition > i && (lastMatch = i);
                    return allow ? writeBuffer() : partialPosition > lastMatch + 1 ? settings.autoclear || buffer.join("") === defaultBuffer ? (input.val() && input.val(""), 
                    clearBuffer(0, len)) : writeBuffer() : (writeBuffer(), input.val(input.val().substring(0, lastMatch + 1))), 
                    partialPosition ? i : firstNonMaskPos;
                }
                var input = $(this), buffer = $.map(mask.split(""), function(c, i) {
                    return "?" != c ? defs[c] ? getPlaceholder(i) : c : void 0;
                }), defaultBuffer = buffer.join(""), focusText = input.val();
                input.data($.mask.dataName, function() {
                    return $.map(buffer, function(c, i) {
                        return tests[i] && c != getPlaceholder(i) ? c : null;
                    }).join("");
                }), input.one("unmask", function() {
                    input.off(".mask").removeData($.mask.dataName);
                }).on("focus.mask", function() {
                    if (!input.prop("readonly")) {
                        clearTimeout(caretTimeoutId);
                        var pos;
                        focusText = input.val(), pos = checkVal(), caretTimeoutId = setTimeout(function() {
                            input.get(0) === document.activeElement && (writeBuffer(), pos == mask.replace("?", "").length ? input.caret(0, pos) : input.caret(pos));
                        }, 10);
                    }
                }).on("blur.mask", blurEvent).on("keydown.mask", keydownEvent).on("keypress.mask", keypressEvent).on("input.mask paste.mask", function() {
                    input.prop("readonly") || setTimeout(function() {
                        var pos = checkVal(!0);
                        input.caret(pos), tryFireCompleted();
                    }, 0);
                }), chrome && android && input.off("input.mask").on("input.mask", androidInputEvent), 
                checkVal();
            });
        }
    });
});
(function() {
    function require(path, parent, orig) {
        var resolved = require.resolve(path);
        if (null == resolved) {
            orig = orig || path;
            parent = parent || "root";
            var err = new Error('Failed to require "' + orig + '" from "' + parent + '"');
            err.path = orig;
            err.parent = parent;
            err.require = true;
            throw err
        }
        var module = require.modules[resolved];
        if (!module.exports) {
            module.exports = {};
            module.client = module.component = true;
            module.call(this, module.exports, require.relative(resolved), module)
        }
        return module.exports
    }
    require.modules = {};
    require.aliases = {};
    require.resolve = function(path) {
        if (path.charAt(0) === "/") path = path.slice(1);
        var paths = [path, path + ".js", path + ".json", path + "/index.js", path + "/index.json"];
        for (var i = 0; i < paths.length; i++) {
            var path = paths[i];
            if (require.modules.hasOwnProperty(path)) return path;
            if (require.aliases.hasOwnProperty(path)) return require.aliases[path]
        }
    };
    require.normalize = function(curr, path) {
        var segs = [];
        if ("." != path.charAt(0)) return path;
        curr = curr.split("/");
        path = path.split("/");
        for (var i = 0; i < path.length; ++i) {
            if (".." == path[i]) {
                curr.pop()
            } else if ("." != path[i] && "" != path[i]) {
                segs.push(path[i])
            }
        }
        return curr.concat(segs).join("/")
    };
    require.register = function(path, definition) {
        require.modules[path] = definition
    };
    require.alias = function(from, to) {
        if (!require.modules.hasOwnProperty(from)) {
            throw new Error('Failed to alias "' + from + '", it does not exist')
        }
        require.aliases[to] = from
    };
    require.relative = function(parent) {
        var p = require.normalize(parent, "..");

        function lastIndexOf(arr, obj) {
            var i = arr.length;
            while (i--) {
                if (arr[i] === obj) return i
            }
            return -1
        }

        function localRequire(path) {
            var resolved = localRequire.resolve(path);
            return require(resolved, parent, path)
        }
        localRequire.resolve = function(path) {
            var c = path.charAt(0);
            if ("/" == c) return path.slice(1);
            if ("." == c) return require.normalize(p, path);
            var segs = parent.split("/");
            var i = lastIndexOf(segs, "deps") + 1;
            if (!i) i = 0;
            path = segs.slice(0, i + 1).join("/") + "/deps/" + path;
            return path
        };
        localRequire.exists = function(path) {
            return require.modules.hasOwnProperty(localRequire.resolve(path))
        };
        return localRequire
    };
    require.register("component-transform-property/index.js", function(exports, require, module) {
        var styles = ["webkitTransform", "MozTransform", "msTransform", "OTransform", "transform"];
        var el = document.createElement("p");
        var style;
        for (var i = 0; i < styles.length; i++) {
            style = styles[i];
            if (null != el.style[style]) {
                module.exports = style;
                break
            }
        }
    });
    require.register("component-has-translate3d/index.js", function(exports, require, module) {
        var prop = require("transform-property");
        if (!prop || !window.getComputedStyle) {
            module.exports = false
        } else {
            var map = {
                webkitTransform: "-webkit-transform",
                OTransform: "-o-transform",
                msTransform: "-ms-transform",
                MozTransform: "-moz-transform",
                transform: "transform"
            };
            var el = document.createElement("div");
            el.style[prop] = "translate3d(1px,1px,1px)";
            document.body.insertBefore(el, null);
            var val = getComputedStyle(el).getPropertyValue(map[prop]);
            document.body.removeChild(el);
            module.exports = null != val && val.length && "none" != val
        }
    });
    require.register("yields-has-transitions/index.js", function(exports, require, module) {
        exports = module.exports = function(el) {
            switch (arguments.length) {
                case 0:
                    return bool;
                case 1:
                    return bool ? transitions(el) : bool
            }
        };

        function transitions(el, styl) {
            if (el.transition) return true;
            styl = window.getComputedStyle(el);
            return !!parseFloat(styl.transitionDuration, 10)
        }
        var styl = document.body.style;
        var bool = "transition" in styl || "webkitTransition" in styl || "MozTransition" in styl || "msTransition" in styl
    });
    require.register("component-event/index.js", function(exports, require, module) {
        var bind = window.addEventListener ? "addEventListener" : "attachEvent",
            unbind = window.removeEventListener ? "removeEventListener" : "detachEvent",
            prefix = bind !== "addEventListener" ? "on" : "";
        exports.bind = function(el, type, fn, capture) {
            el[bind](prefix + type, fn, capture || false);
            return fn
        };
        exports.unbind = function(el, type, fn, capture) {
            el[unbind](prefix + type, fn, capture || false);
            return fn
        }
    });
    require.register("ecarter-css-emitter/index.js", function(exports, require, module) {
        var events = require("event");
        var watch = ["transitionend", "webkitTransitionEnd", "oTransitionEnd", "MSTransitionEnd", "animationend", "webkitAnimationEnd", "oAnimationEnd", "MSAnimationEnd"];
        module.exports = CssEmitter;

        function CssEmitter(element) {
            if (!(this instanceof CssEmitter)) return new CssEmitter(element);
            this.el = element
        }
        CssEmitter.prototype.bind = function(fn) {
            for (var i = 0; i < watch.length; i++) {
                events.bind(this.el, watch[i], fn)
            }
            return this
        };
        CssEmitter.prototype.unbind = function(fn) {
            for (var i = 0; i < watch.length; i++) {
                events.unbind(this.el, watch[i], fn)
            }
            return this
        };
        CssEmitter.prototype.once = function(fn) {
            var self = this;

            function on() {
                self.unbind(on);
                fn.apply(self.el, arguments)
            }
            self.bind(on);
            return this
        }
    });
    require.register("component-once/index.js", function(exports, require, module) {
        var n = 0;
        var global = function() {
            return this
        }();
        module.exports = function(fn) {
            var id = n++;
            var called;

            function once() {
                if (this == global) {
                    if (called) return;
                    called = true;
                    return fn.apply(this, arguments)
                }
                var key = "__called_" + id + "__";
                if (this[key]) return;
                this[key] = true;
                return fn.apply(this, arguments)
            }
            return once
        }
    });
    require.register("yields-after-transition/index.js", function(exports, require, module) {
        var has = require("has-transitions"),
            emitter = require("css-emitter"),
            once = require("once");
        var supported = has();
        module.exports = after;

        function after(el, fn) {
            if (!supported || !has(el)) return fn();
            emitter(el).bind(fn);
            return fn
        }
        after.once = function(el, fn) {
            var callback = once(fn);
            after(el, fn = function() {
                emitter(el).unbind(fn);
                callback()
            })
        }
    });
    require.register("component-emitter/index.js", function(exports, require, module) {
        module.exports = Emitter;

        function Emitter(obj) {
            if (obj) return mixin(obj)
        }

        function mixin(obj) {
            for (var key in Emitter.prototype) {
                obj[key] = Emitter.prototype[key]
            }
            return obj
        }
        Emitter.prototype.on = Emitter.prototype.addEventListener = function(event, fn) {
            this._callbacks = this._callbacks || {};
            (this._callbacks[event] = this._callbacks[event] || []).push(fn);
            return this
        };
        Emitter.prototype.once = function(event, fn) {
            var self = this;
            this._callbacks = this._callbacks || {};

            function on() {
                self.off(event, on);
                fn.apply(this, arguments)
            }
            on.fn = fn;
            this.on(event, on);
            return this
        };
        Emitter.prototype.off = Emitter.prototype.removeListener = Emitter.prototype.removeAllListeners = Emitter.prototype.removeEventListener = function(event, fn) {
            this._callbacks = this._callbacks || {};
            if (0 == arguments.length) {
                this._callbacks = {};
                return this
            }
            var callbacks = this._callbacks[event];
            if (!callbacks) return this;
            if (1 == arguments.length) {
                delete this._callbacks[event];
                return this
            }
            var cb;
            for (var i = 0; i < callbacks.length; i++) {
                cb = callbacks[i];
                if (cb === fn || cb.fn === fn) {
                    callbacks.splice(i, 1);
                    break
                }
            }
            return this
        };
        Emitter.prototype.emit = function(event) {
            this._callbacks = this._callbacks || {};
            var args = [].slice.call(arguments, 1),
                callbacks = this._callbacks[event];
            if (callbacks) {
                callbacks = callbacks.slice(0);
                for (var i = 0, len = callbacks.length; i < len; ++i) {
                    callbacks[i].apply(this, args)
                }
            }
            return this
        };
        Emitter.prototype.listeners = function(event) {
            this._callbacks = this._callbacks || {};
            return this._callbacks[event] || []
        };
        Emitter.prototype.hasListeners = function(event) {
            return !!this.listeners(event).length
        }
    });
    require.register("yields-css-ease/index.js", function(exports, require, module) {
        module.exports = {
            "in": "ease-in",
            out: "ease-out",
            "in-out": "ease-in-out",
            snap: "cubic-bezier(0,1,.5,1)",
            linear: "cubic-bezier(0.250, 0.250, 0.750, 0.750)",
            "ease-in-quad": "cubic-bezier(0.550, 0.085, 0.680, 0.530)",
            "ease-in-cubic": "cubic-bezier(0.550, 0.055, 0.675, 0.190)",
            "ease-in-quart": "cubic-bezier(0.895, 0.030, 0.685, 0.220)",
            "ease-in-quint": "cubic-bezier(0.755, 0.050, 0.855, 0.060)",
            "ease-in-sine": "cubic-bezier(0.470, 0.000, 0.745, 0.715)",
            "ease-in-expo": "cubic-bezier(0.950, 0.050, 0.795, 0.035)",
            "ease-in-circ": "cubic-bezier(0.600, 0.040, 0.980, 0.335)",
            "ease-in-back": "cubic-bezier(0.600, -0.280, 0.735, 0.045)",
            "ease-out-quad": "cubic-bezier(0.250, 0.460, 0.450, 0.940)",
            "ease-out-cubic": "cubic-bezier(0.215, 0.610, 0.355, 1.000)",
            "ease-out-quart": "cubic-bezier(0.165, 0.840, 0.440, 1.000)",
            "ease-out-quint": "cubic-bezier(0.230, 1.000, 0.320, 1.000)",
            "ease-out-sine": "cubic-bezier(0.390, 0.575, 0.565, 1.000)",
            "ease-out-expo": "cubic-bezier(0.190, 1.000, 0.220, 1.000)",
            "ease-out-circ": "cubic-bezier(0.075, 0.820, 0.165, 1.000)",
            "ease-out-back": "cubic-bezier(0.175, 0.885, 0.320, 1.275)",
            "ease-out-quad": "cubic-bezier(0.455, 0.030, 0.515, 0.955)",
            "ease-out-cubic": "cubic-bezier(0.645, 0.045, 0.355, 1.000)",
            "ease-in-out-quart": "cubic-bezier(0.770, 0.000, 0.175, 1.000)",
            "ease-in-out-quint": "cubic-bezier(0.860, 0.000, 0.070, 1.000)",
            "ease-in-out-sine": "cubic-bezier(0.445, 0.050, 0.550, 0.950)",
            "ease-in-out-expo": "cubic-bezier(1.000, 0.000, 0.000, 1.000)",
            "ease-in-out-circ": "cubic-bezier(0.785, 0.135, 0.150, 0.860)",
            "ease-in-out-back": "cubic-bezier(0.680, -0.550, 0.265, 1.550)"
        }
    });
    require.register("component-query/index.js", function(exports, require, module) {
        function one(selector, el) {
            return el.querySelector(selector)
        }
        exports = module.exports = function(selector, el) {
            el = el || document;
            return one(selector, el)
        };
        exports.all = function(selector, el) {
            el = el || document;
            return el.querySelectorAll(selector)
        };
        exports.engine = function(obj) {
            if (!obj.one) throw new Error(".one callback required");
            if (!obj.all) throw new Error(".all callback required");
            one = obj.one;
            exports.all = obj.all;
            return exports
        }
    });
    require.register("move/index.js", function(exports, require, module) {
        var after = require("after-transition");
        var has3d = require("has-translate3d");
        var Emitter = require("emitter");
        var ease = require("css-ease");
        var query = require("query");
        var translate = has3d ? ["translate3d(", ", 0)"] : ["translate(", ")"];
        module.exports = Move;
        var style = window.getComputedStyle || window.currentStyle;
        Move.version = "0.3.2";
        Move.ease = ease;
        Move.defaults = {
            duration: 500
        };
        Move.select = function(selector) {
            if ("string" != typeof selector) return selector;
            return query(selector)
        };

        function Move(el) {
            if (!(this instanceof Move)) return new Move(el);
            if ("string" == typeof el) el = query(el);
            if (!el) throw new TypeError("Move must be initialized with element or selector");
            this.el = el;
            this._props = {};
            this._rotate = 0;
            this._transitionProps = [];
            this._transforms = [];
            this.duration(Move.defaults.duration)
        }
        Emitter(Move.prototype);
        Move.prototype.transform = function(transform) {
            this._transforms.push(transform);
            return this
        };
        Move.prototype.skew = function(x, y) {
            return this.transform("skew(" + x + "deg, " + (y || 0) + "deg)")
        };
        Move.prototype.skewX = function(n) {
            return this.transform("skewX(" + n + "deg)")
        };
        Move.prototype.skewY = function(n) {
            return this.transform("skewY(" + n + "deg)")
        };
        Move.prototype.translate = Move.prototype.to = function(x, y) {
            return this.transform(translate.join("" + x + "px, " + (y || 0) + "px"))
        };
        Move.prototype.translateX = Move.prototype.x = function(n) {
            return this.transform("translateX(" + n + "px)")
        };
        Move.prototype.translateY = Move.prototype.y = function(n) {
            return this.transform("translateY(" + n + "px)")
        };
        Move.prototype.scale = function(x, y) {
            return this.transform("scale(" + x + ", " + (y || x) + ")")
        };
        Move.prototype.scaleX = function(n) {
            return this.transform("scaleX(" + n + ")")
        };
        Move.prototype.matrix = function(m11, m12, m21, m22, m31, m32) {
            return this.transform("matrix(" + [m11, m12, m21, m22, m31, m32].join(",") + ")")
        };
        Move.prototype.scaleY = function(n) {
            return this.transform("scaleY(" + n + ")")
        };
        Move.prototype.rotate = function(n) {
            return this.transform("rotate(" + n + "deg)")
        };
        Move.prototype.ease = function(fn) {
            fn = ease[fn] || fn || "ease";
            return this.setVendorProperty("transition-timing-function", fn)
        };
        Move.prototype.animate = function(name, props) {
            for (var i in props) {
                if (props.hasOwnProperty(i)) {
                    this.setVendorProperty("animation-" + i, props[i])
                }
            }
            return this.setVendorProperty("animation-name", name)
        };
        Move.prototype.duration = function(n) {
            n = this._duration = "string" == typeof n ? parseFloat(n) * 1e3 : n;
            return this.setVendorProperty("transition-duration", n + "ms")
        };
        Move.prototype.delay = function(n) {
            n = "string" == typeof n ? parseFloat(n) * 1e3 : n;
            return this.setVendorProperty("transition-delay", n + "ms")
        };
        Move.prototype.setProperty = function(prop, val) {
            this._props[prop] = val;
            return this
        };
        Move.prototype.setVendorProperty = function(prop, val) {
            this.setProperty("-webkit-" + prop, val);
            this.setProperty("-moz-" + prop, val);
            this.setProperty("-ms-" + prop, val);
            this.setProperty("-o-" + prop, val);
            return this
        };
        Move.prototype.set = function(prop, val) {
            this.transition(prop);
            this._props[prop] = val;
            return this
        };
        Move.prototype.add = function(prop, val) {
            if (!style) return;
            var self = this;
            return this.on("start", function() {
                var curr = parseInt(self.current(prop), 10);
                self.set(prop, curr + val + "px")
            })
        };
        Move.prototype.sub = function(prop, val) {
            if (!style) return;
            var self = this;
            return this.on("start", function() {
                var curr = parseInt(self.current(prop), 10);
                self.set(prop, curr - val + "px")
            })
        };
        Move.prototype.current = function(prop) {
            return style(this.el).getPropertyValue(prop)
        };
        Move.prototype.transition = function(prop) {
            if (!this._transitionProps.indexOf(prop)) return this;
            this._transitionProps.push(prop);
            return this
        };
        Move.prototype.applyProperties = function() {
            for (var prop in this._props) {
                this.el.style.setProperty(prop, this._props[prop], "")
            }
            return this
        };
        Move.prototype.move = Move.prototype.select = function(selector) {
            this.el = Move.select(selector);
            return this
        };
        Move.prototype.then = function(fn) {
            if (fn instanceof Move) {
                this.on("end", function() {
                    fn.end()
                })
            } else if ("function" == typeof fn) {
                this.on("end", fn)
            } else {
                var clone = new Move(this.el);
                clone._transforms = this._transforms.slice(0);
                this.then(clone);
                clone.parent = this;
                return clone
            }
            return this
        };
        Move.prototype.pop = function() {
            return this.parent
        };
        Move.prototype.reset = function() {
            this.el.style.webkitTransitionDuration = this.el.style.mozTransitionDuration = this.el.style.msTransitionDuration = this.el.style.oTransitionDuration = "";
            return this
        };
        Move.prototype.end = function(fn) {
            var self = this;
            this.emit("start");
            if (this._transforms.length) {
                this.setVendorProperty("transform", this._transforms.join(" "))
            }
            this.setVendorProperty("transition-properties", this._transitionProps.join(", "));
            this.applyProperties();
            if (fn) this.then(fn);
            after.once(this.el, function() {
                self.reset();
                self.emit("end")
            });
            return this
        }
    });
    require.alias("component-has-translate3d/index.js", "move/deps/has-translate3d/index.js");
    require.alias("component-has-translate3d/index.js", "has-translate3d/index.js");
    require.alias("component-transform-property/index.js", "component-has-translate3d/deps/transform-property/index.js");
    require.alias("yields-after-transition/index.js", "move/deps/after-transition/index.js");
    require.alias("yields-after-transition/index.js", "move/deps/after-transition/index.js");
    require.alias("yields-after-transition/index.js", "after-transition/index.js");
    require.alias("yields-has-transitions/index.js", "yields-after-transition/deps/has-transitions/index.js");
    require.alias("yields-has-transitions/index.js", "yields-after-transition/deps/has-transitions/index.js");
    require.alias("yields-has-transitions/index.js", "yields-has-transitions/index.js");
    require.alias("ecarter-css-emitter/index.js", "yields-after-transition/deps/css-emitter/index.js");
    require.alias("component-event/index.js", "ecarter-css-emitter/deps/event/index.js");
    require.alias("component-once/index.js", "yields-after-transition/deps/once/index.js");
    require.alias("yields-after-transition/index.js", "yields-after-transition/index.js");
    require.alias("component-emitter/index.js", "move/deps/emitter/index.js");
    require.alias("component-emitter/index.js", "emitter/index.js");
    require.alias("yields-css-ease/index.js", "move/deps/css-ease/index.js");
    require.alias("yields-css-ease/index.js", "move/deps/css-ease/index.js");
    require.alias("yields-css-ease/index.js", "css-ease/index.js");
    require.alias("yields-css-ease/index.js", "yields-css-ease/index.js");
    require.alias("component-query/index.js", "move/deps/query/index.js");
    require.alias("component-query/index.js", "query/index.js");
    if (typeof exports == "object") {
        module.exports = require("move")
    } else if (typeof define == "function" && define.amd) {
        define(function() {
            return require("move")
        })
    } else {
        this["move"] = require("move")
    }
})();
/**
 * vivus - JavaScript library to make drawing animation on SVG
 * @version v0.2.3
 * @link https://github.com/maxwellito/vivus
 * @license MIT
 */
"use strict";!function(t,e){function r(r){if("undefined"==typeof r)throw new Error('Pathformer [constructor]: "element" parameter is required');if(r.constructor===String&&(r=e.getElementById(r),!r))throw new Error('Pathformer [constructor]: "element" parameter is not related to an existing ID');if(!(r.constructor instanceof t.SVGElement||/^svg$/i.test(r.nodeName)))throw new Error('Pathformer [constructor]: "element" parameter must be a string or a SVGelement');this.el=r,this.scan(r)}function n(t,e,r){this.isReady=!1,this.setElement(t,e),this.setOptions(e),this.setCallback(r),this.isReady&&this.init()}r.prototype.TYPES=["line","ellipse","circle","polygon","polyline","rect"],r.prototype.ATTR_WATCH=["cx","cy","points","r","rx","ry","x","x1","x2","y","y1","y2"],r.prototype.scan=function(t){for(var e,r,n,i,a=t.querySelectorAll(this.TYPES.join(",")),o=0;o<a.length;o++)r=a[o],e=this[r.tagName.toLowerCase()+"ToPath"],n=e(this.parseAttr(r.attributes)),i=this.pathMaker(r,n),r.parentNode.replaceChild(i,r)},r.prototype.lineToPath=function(t){var e={};return e.d="M"+t.x1+","+t.y1+"L"+t.x2+","+t.y2,e},r.prototype.rectToPath=function(t){var e={},r=parseFloat(t.x)||0,n=parseFloat(t.y)||0,i=parseFloat(t.width)||0,a=parseFloat(t.height)||0;return e.d="M"+r+" "+n+" ",e.d+="L"+(r+i)+" "+n+" ",e.d+="L"+(r+i)+" "+(n+a)+" ",e.d+="L"+r+" "+(n+a)+" Z",e},r.prototype.polylineToPath=function(t){var e,r,n={},i=t.points.trim().split(" ");if(-1===t.points.indexOf(",")){var a=[];for(e=0;e<i.length;e+=2)a.push(i[e]+","+i[e+1]);i=a}for(r="M"+i[0],e=1;e<i.length;e++)-1!==i[e].indexOf(",")&&(r+="L"+i[e]);return n.d=r,n},r.prototype.polygonToPath=function(t){var e=r.prototype.polylineToPath(t);return e.d+="Z",e},r.prototype.ellipseToPath=function(t){var e=t.cx-t.rx,r=t.cy,n=parseFloat(t.cx)+parseFloat(t.rx),i=t.cy,a={};return a.d="M"+e+","+r+"A"+t.rx+","+t.ry+" 0,1,1 "+n+","+i+"A"+t.rx+","+t.ry+" 0,1,1 "+e+","+i,a},r.prototype.circleToPath=function(t){var e={},r=t.cx-t.r,n=t.cy,i=parseFloat(t.cx)+parseFloat(t.r),a=t.cy;return e.d="M"+r+","+n+"A"+t.r+","+t.r+" 0,1,1 "+i+","+a+"A"+t.r+","+t.r+" 0,1,1 "+r+","+a,e},r.prototype.pathMaker=function(t,r){var n,i,a=e.createElementNS("http://www.w3.org/2000/svg","path");for(n=0;n<t.attributes.length;n++)i=t.attributes[n],-1===this.ATTR_WATCH.indexOf(i.name)&&a.setAttribute(i.name,i.value);for(n in r)a.setAttribute(n,r[n]);return a},r.prototype.parseAttr=function(t){for(var e,r={},n=0;n<t.length;n++){if(e=t[n],-1!==this.ATTR_WATCH.indexOf(e.name)&&-1!==e.value.indexOf("%"))throw new Error("Pathformer [parseAttr]: a SVG shape got values in percentage. This cannot be transformed into 'path' tags. Please use 'viewBox'.");r[e.name]=e.value}return r};var i,a,o;n.LINEAR=function(t){return t},n.EASE=function(t){return-Math.cos(t*Math.PI)/2+.5},n.EASE_OUT=function(t){return 1-Math.pow(1-t,3)},n.EASE_IN=function(t){return Math.pow(t,3)},n.EASE_OUT_BOUNCE=function(t){var e=-Math.cos(.5*t*Math.PI)+1,r=Math.pow(e,1.5),n=Math.pow(1-t,2),i=-Math.abs(Math.cos(2.5*r*Math.PI))+1;return 1-n+i*n},n.prototype.setElement=function(r,n){if("undefined"==typeof r)throw new Error('Vivus [constructor]: "element" parameter is required');if(r.constructor===String&&(r=e.getElementById(r),!r))throw new Error('Vivus [constructor]: "element" parameter is not related to an existing ID');if(this.parentEl=r,n&&n.file){var i=e.createElement("object");i.setAttribute("type","image/svg+xml"),i.setAttribute("data",n.file),i.setAttribute("width","100%"),i.setAttribute("height","100%"),r.appendChild(i),r=i}switch(r.constructor){case t.SVGSVGElement:case t.SVGElement:this.el=r,this.isReady=!0;break;case t.HTMLObjectElement:if(this.el=r.contentDocument&&r.contentDocument.querySelector("svg"),this.el)return this.isReady=!0,void 0;var a=this;r.addEventListener("load",function(){if(a.el=r.contentDocument&&r.contentDocument.querySelector("svg"),!a.el)throw new Error("Vivus [constructor]: object loaded does not contain any SVG");a.isReady=!0,a.init()});break;default:throw new Error('Vivus [constructor]: "element" parameter is not valid (or miss the "file" attribute)')}},n.prototype.setOptions=function(e){var r=["delayed","async","oneByOne","scenario","scenario-sync"],i=["inViewport","manual","autostart"];if(void 0!==e&&e.constructor!==Object)throw new Error('Vivus [constructor]: "options" parameter must be an object');if(e=e||{},e.type&&-1===r.indexOf(e.type))throw new Error("Vivus [constructor]: "+e.type+" is not an existing animation `type`");if(this.type=e.type||r[0],e.start&&-1===i.indexOf(e.start))throw new Error("Vivus [constructor]: "+e.start+" is not an existing `start` option");if(this.start=e.start||i[0],this.isIE=-1!==t.navigator.userAgent.indexOf("MSIE")||-1!==t.navigator.userAgent.indexOf("Trident/")||-1!==t.navigator.userAgent.indexOf("Edge/"),this.duration=o(e.duration,120),this.delay=o(e.delay,null),this.dashGap=o(e.dashGap,2),this.forceRender=e.hasOwnProperty("forceRender")?!!e.forceRender:this.isIE,this.selfDestroy=!!e.selfDestroy,this.onReady=e.onReady,this.ignoreInvisible=e.hasOwnProperty("ignoreInvisible")?!!e.ignoreInvisible:!1,this.animTimingFunction=e.animTimingFunction||n.LINEAR,this.pathTimingFunction=e.pathTimingFunction||n.LINEAR,this.delay>=this.duration)throw new Error("Vivus [constructor]: delay must be shorter than duration")},n.prototype.setCallback=function(t){if(t&&t.constructor!==Function)throw new Error('Vivus [constructor]: "callback" parameter must be a function');this.callback=t||function(){}},n.prototype.mapping=function(){var e,r,n,i,a,s,h,u;for(u=s=h=0,r=this.el.querySelectorAll("path"),e=0;e<r.length;e++)n=r[e],this.isInvisible(n)||(a={el:n,length:Math.ceil(n.getTotalLength())},isNaN(a.length)?t.console&&console.warn&&console.warn("Vivus [mapping]: cannot retrieve a path element length",n):(s+=a.length,this.map.push(a),n.style.strokeDasharray=a.length+" "+(a.length+this.dashGap),n.style.strokeDashoffset=a.length,this.isIE&&(a.length+=this.dashGap),this.renderPath(e)));for(s=0===s?1:s,this.delay=null===this.delay?this.duration/3:this.delay,this.delayUnit=this.delay/(r.length>1?r.length-1:1),e=0;e<this.map.length;e++){switch(a=this.map[e],this.type){case"delayed":a.startAt=this.delayUnit*e,a.duration=this.duration-this.delay;break;case"oneByOne":a.startAt=h/s*this.duration,a.duration=a.length/s*this.duration;break;case"async":a.startAt=0,a.duration=this.duration;break;case"scenario-sync":n=r[e],i=this.parseAttr(n),a.startAt=u+(o(i["data-delay"],this.delayUnit)||0),a.duration=o(i["data-duration"],this.duration),u=void 0!==i["data-async"]?a.startAt:a.startAt+a.duration,this.frameLength=Math.max(this.frameLength,a.startAt+a.duration);break;case"scenario":n=r[e],i=this.parseAttr(n),a.startAt=o(i["data-start"],this.delayUnit)||0,a.duration=o(i["data-duration"],this.duration),this.frameLength=Math.max(this.frameLength,a.startAt+a.duration)}h+=a.length,this.frameLength=this.frameLength||this.duration}},n.prototype.drawer=function(){var t=this;this.currentFrame+=this.speed,this.currentFrame<=0?(this.stop(),this.reset(),this.callback(this)):this.currentFrame>=this.frameLength?(this.stop(),this.currentFrame=this.frameLength,this.trace(),this.selfDestroy&&this.destroy(),this.callback(this)):(this.trace(),this.handle=i(function(){t.drawer()}))},n.prototype.trace=function(){var t,e,r,n;for(n=this.animTimingFunction(this.currentFrame/this.frameLength)*this.frameLength,t=0;t<this.map.length;t++)r=this.map[t],e=(n-r.startAt)/r.duration,e=this.pathTimingFunction(Math.max(0,Math.min(1,e))),r.progress!==e&&(r.progress=e,r.el.style.strokeDashoffset=Math.floor(r.length*(1-e)),this.renderPath(t))},n.prototype.renderPath=function(t){if(this.forceRender&&this.map&&this.map[t]){var e=this.map[t],r=e.el.cloneNode(!0);e.el.parentNode.replaceChild(r,e.el),e.el=r}},n.prototype.init=function(){this.frameLength=0,this.currentFrame=0,this.map=[],new r(this.el),this.mapping(),this.starter(),this.onReady&&this.onReady(this)},n.prototype.starter=function(){switch(this.start){case"manual":return;case"autostart":this.play();break;case"inViewport":var e=this,r=function(){e.isInViewport(e.parentEl,1)&&(e.play(),t.removeEventListener("scroll",r))};t.addEventListener("scroll",r),r()}},n.prototype.getStatus=function(){return 0===this.currentFrame?"start":this.currentFrame===this.frameLength?"end":"progress"},n.prototype.reset=function(){return this.setFrameProgress(0)},n.prototype.finish=function(){return this.setFrameProgress(1)},n.prototype.setFrameProgress=function(t){return t=Math.min(1,Math.max(0,t)),this.currentFrame=Math.round(this.frameLength*t),this.trace(),this},n.prototype.play=function(t){if(t&&"number"!=typeof t)throw new Error("Vivus [play]: invalid speed");return this.speed=t||1,this.handle||this.drawer(),this},n.prototype.stop=function(){return this.handle&&(a(this.handle),delete this.handle),this},n.prototype.destroy=function(){var t,e;for(t=0;t<this.map.length;t++)e=this.map[t],e.el.style.strokeDashoffset=null,e.el.style.strokeDasharray=null,this.renderPath(t)},n.prototype.isInvisible=function(t){var e,r=t.getAttribute("data-ignore");return null!==r?"false"!==r:this.ignoreInvisible?(e=t.getBoundingClientRect(),!e.width&&!e.height):!1},n.prototype.parseAttr=function(t){var e,r={};if(t&&t.attributes)for(var n=0;n<t.attributes.length;n++)e=t.attributes[n],r[e.name]=e.value;return r},n.prototype.isInViewport=function(t,e){var r=this.scrollY(),n=r+this.getViewportH(),i=t.getBoundingClientRect(),a=i.height,o=r+i.top,s=o+a;return e=e||0,n>=o+a*e&&s>=r},n.prototype.docElem=t.document.documentElement,n.prototype.getViewportH=function(){var e=this.docElem.clientHeight,r=t.innerHeight;return r>e?r:e},n.prototype.scrollY=function(){return t.pageYOffset||this.docElem.scrollTop},i=function(){return t.requestAnimationFrame||t.webkitRequestAnimationFrame||t.mozRequestAnimationFrame||t.oRequestAnimationFrame||t.msRequestAnimationFrame||function(e){return t.setTimeout(e,1e3/60)}}(),a=function(){return t.cancelAnimationFrame||t.webkitCancelAnimationFrame||t.mozCancelAnimationFrame||t.oCancelAnimationFrame||t.msCancelAnimationFrame||function(e){return t.clearTimeout(e)}}(),o=function(t,e){var r=parseInt(t,10);return r>=0?r:e},"function"==typeof define&&define.amd?define([],function(){return n}):"object"==typeof exports?module.exports=n:t.Vivus=n}(window,document);
/*
 * Mailcheck https://github.com/mailcheck/mailcheck
 * Author
 * Derrick Ko (@derrickko)
 *
 * Released under the MIT License.
 *
 * v 1.1.1
 */

var Mailcheck = {
  domainThreshold: 2,
  secondLevelThreshold: 2,
  topLevelThreshold: 2,

  defaultDomains: ['msn.com', 'bellsouth.net',
    'telus.net', 'comcast.net', 'optusnet.com.au',
    'earthlink.net', 'qq.com', 'sky.com', 'icloud.com',
    'mac.com', 'sympatico.ca', 'googlemail.com',
    'att.net', 'xtra.co.nz', 'web.de',
    'cox.net', 'gmail.com', 'ymail.com',
    'aim.com', 'rogers.com', 'verizon.net',
    'rocketmail.com', 'google.com', 'optonline.net',
    'sbcglobal.net', 'aol.com', 'me.com', 'btinternet.com',
    'charter.net', 'shaw.ca'],

  defaultSecondLevelDomains: ["yahoo", "hotmail", "mail", "live", "outlook", "gmx"],

  defaultTopLevelDomains: ["com", "com.au", "com.tw", "ca", "co.nz", "co.uk", "de",
    "fr", "it", "ru", "net", "org", "edu", "gov", "jp", "nl", "kr", "se", "eu",
    "ie", "co.il", "us", "at", "be", "dk", "hk", "es", "gr", "ch", "no", "cz",
    "in", "net", "net.au", "info", "biz", "mil", "co.jp", "sg", "hu"],

  run: function(opts) {
    opts.domains = opts.domains || Mailcheck.defaultDomains;
    opts.secondLevelDomains = opts.secondLevelDomains || Mailcheck.defaultSecondLevelDomains;
    opts.topLevelDomains = opts.topLevelDomains || Mailcheck.defaultTopLevelDomains;
    opts.distanceFunction = opts.distanceFunction || Mailcheck.sift3Distance;

    var defaultCallback = function(result){ return result };
    var suggestedCallback = opts.suggested || defaultCallback;
    var emptyCallback = opts.empty || defaultCallback;

    var result = Mailcheck.suggest(Mailcheck.encodeEmail(opts.email), opts.domains, opts.secondLevelDomains, opts.topLevelDomains, opts.distanceFunction);

    return result ? suggestedCallback(result) : emptyCallback()
  },

  suggest: function(email, domains, secondLevelDomains, topLevelDomains, distanceFunction) {
    email = email.toLowerCase();

    var emailParts = this.splitEmail(email);

    if (secondLevelDomains && topLevelDomains) {
        // If the email is a valid 2nd-level + top-level, do not suggest anything.
        if (secondLevelDomains.indexOf(emailParts.secondLevelDomain) !== -1 && topLevelDomains.indexOf(emailParts.topLevelDomain) !== -1) {
            return false;
        }
    }

    var closestDomain = this.findClosestDomain(emailParts.domain, domains, distanceFunction, this.domainThreshold);

    if (closestDomain) {
      if (closestDomain == emailParts.domain) {
        // The email address exactly matches one of the supplied domains; do not return a suggestion.
        return false;
      } else {
        // The email address closely matches one of the supplied domains; return a suggestion
        return { address: emailParts.address, domain: closestDomain, full: emailParts.address + "@" + closestDomain };
      }
    }

    // The email address does not closely match one of the supplied domains
    var closestSecondLevelDomain = this.findClosestDomain(emailParts.secondLevelDomain, secondLevelDomains, distanceFunction, this.secondLevelThreshold);
    var closestTopLevelDomain    = this.findClosestDomain(emailParts.topLevelDomain, topLevelDomains, distanceFunction, this.topLevelThreshold);

    if (emailParts.domain) {
      var closestDomain = emailParts.domain;
      var rtrn = false;

      if(closestSecondLevelDomain && closestSecondLevelDomain != emailParts.secondLevelDomain) {
        // The email address may have a mispelled second-level domain; return a suggestion
        closestDomain = closestDomain.replace(emailParts.secondLevelDomain, closestSecondLevelDomain);
        rtrn = true;
      }

      if(closestTopLevelDomain && closestTopLevelDomain != emailParts.topLevelDomain) {
        // The email address may have a mispelled top-level domain; return a suggestion
        closestDomain = closestDomain.replace(emailParts.topLevelDomain, closestTopLevelDomain);
        rtrn = true;
      }

      if (rtrn == true) {
        return { address: emailParts.address, domain: closestDomain, full: emailParts.address + "@" + closestDomain };
      }
    }

    /* The email address exactly matches one of the supplied domains, does not closely
     * match any domain and does not appear to simply have a mispelled top-level domain,
     * or is an invalid email address; do not return a suggestion.
     */
    return false;
  },

  findClosestDomain: function(domain, domains, distanceFunction, threshold) {
    threshold = threshold || this.topLevelThreshold;
    var dist;
    var minDist = 99;
    var closestDomain = null;

    if (!domain || !domains) {
      return false;
    }
    if(!distanceFunction) {
      distanceFunction = this.sift3Distance;
    }

    for (var i = 0; i < domains.length; i++) {
      if (domain === domains[i]) {
        return domain;
      }
      dist = distanceFunction(domain, domains[i]);
      if (dist < minDist) {
        minDist = dist;
        closestDomain = domains[i];
      }
    }

    if (minDist <= threshold && closestDomain !== null) {
      return closestDomain;
    } else {
      return false;
    }
  },

  sift3Distance: function(s1, s2) {
    // sift3: http://siderite.blogspot.com/2007/04/super-fast-and-accurate-string-distance.html
    if (s1 == null || s1.length === 0) {
      if (s2 == null || s2.length === 0) {
        return 0;
      } else {
        return s2.length;
      }
    }

    if (s2 == null || s2.length === 0) {
      return s1.length;
    }

    var c = 0;
    var offset1 = 0;
    var offset2 = 0;
    var lcs = 0;
    var maxOffset = 5;

    while ((c + offset1 < s1.length) && (c + offset2 < s2.length)) {
      if (s1.charAt(c + offset1) == s2.charAt(c + offset2)) {
        lcs++;
      } else {
        offset1 = 0;
        offset2 = 0;
        for (var i = 0; i < maxOffset; i++) {
          if ((c + i < s1.length) && (s1.charAt(c + i) == s2.charAt(c))) {
            offset1 = i;
            break;
          }
          if ((c + i < s2.length) && (s1.charAt(c) == s2.charAt(c + i))) {
            offset2 = i;
            break;
          }
        }
      }
      c++;
    }
    return (s1.length + s2.length) /2 - lcs;
  },

  splitEmail: function(email) {
    var parts = email.trim().split('@');

    if (parts.length < 2) {
      return false;
    }

    for (var i = 0; i < parts.length; i++) {
      if (parts[i] === '') {
        return false;
      }
    }

    var domain = parts.pop();
    var domainParts = domain.split('.');
    var sld = '';
    var tld = '';

    if (domainParts.length == 0) {
      // The address does not have a top-level domain
      return false;
    } else if (domainParts.length == 1) {
      // The address has only a top-level domain (valid under RFC)
      tld = domainParts[0];
    } else {
      // The address has a domain and a top-level domain
      sld = domainParts[0];
      for (var i = 1; i < domainParts.length; i++) {
        tld += domainParts[i] + '.';
      }
      tld = tld.substring(0, tld.length - 1);
    }

    return {
      topLevelDomain: tld,
      secondLevelDomain: sld,
      domain: domain,
      address: parts.join('@')
    }
  },

  // Encode the email address to prevent XSS but leave in valid
  // characters, following this official spec:
  // http://en.wikipedia.org/wiki/Email_address#Syntax
  encodeEmail: function(email) {
    var result = encodeURI(email);
    result = result.replace('%20', ' ').replace('%25', '%').replace('%5E', '^')
                   .replace('%60', '`').replace('%7B', '{').replace('%7C', '|')
                   .replace('%7D', '}');
    return result;
  }
};

// Export the mailcheck object if we're in a CommonJS env (e.g. Node).
// Modeled off of Underscore.js.
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Mailcheck;
}

// Support AMD style definitions
// Based on jQuery (see http://stackoverflow.com/a/17954882/1322410)
if (typeof define === "function" && define.amd) {
  define("mailcheck", [], function() {
    return Mailcheck;
  });
}

if (typeof window !== 'undefined' && window.jQuery) {
  (function($){
    $.fn.mailcheck = function(opts) {
      var self = this;
      if (opts.suggested) {
        var oldSuggested = opts.suggested;
        opts.suggested = function(result) {
          oldSuggested(self, result);
        };
      }

      if (opts.empty) {
        var oldEmpty = opts.empty;
        opts.empty = function() {
          oldEmpty.call(null, self);
        };
      }

      opts.email = this.val();
      Mailcheck.run(opts);
    }
  })(jQuery);
}

/******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};

/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {

/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId])
/******/ 			return installedModules[moduleId].exports;

/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			exports: {},
/******/ 			id: moduleId,
/******/ 			loaded: false
/******/ 		};

/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);

/******/ 		// Flag the module as loaded
/******/ 		module.loaded = true;

/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}


/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;

/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;

/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "/";

/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(0);
/******/ })
/************************************************************************/
/******/ ([
/* 0 */
/***/ function(module, exports, __webpack_require__) {

	module.exports = __webpack_require__(1);


/***/ },
/* 1 */
/***/ function(module, exports, __webpack_require__) {

	'use strict';

	Object.defineProperty(exports, '__esModule', {
		value: true
	});

	var _createClass = (function () { function defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ('value' in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } } return function (Constructor, protoProps, staticProps) { if (protoProps) defineProperties(Constructor.prototype, protoProps); if (staticProps) defineProperties(Constructor, staticProps); return Constructor; }; })();

	var _get = function get(_x, _x2, _x3) { var _again = true; _function: while (_again) { var object = _x, property = _x2, receiver = _x3; desc = parent = getter = undefined; _again = false; var desc = Object.getOwnPropertyDescriptor(object, property); if (desc === undefined) { var parent = Object.getPrototypeOf(object); if (parent === null) { return undefined; } else { _x = parent; _x2 = property; _x3 = receiver; _again = true; continue _function; } } else if ('value' in desc) { return desc.value; } else { var getter = desc.get; if (getter === undefined) { return undefined; } return getter.call(receiver); } } };

	function _interopRequireDefault(obj) { return obj && obj.__esModule ? obj : { 'default': obj }; }

	function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError('Cannot call a class as a function'); } }

	function _inherits(subClass, superClass) { if (typeof superClass !== 'function' && superClass !== null) { throw new TypeError('Super expression must either be null or a function, not ' + typeof superClass); } subClass.prototype = Object.create(superClass && superClass.prototype, { constructor: { value: subClass, enumerable: false, writable: true, configurable: true } }); if (superClass) subClass.__proto__ = superClass; }

	var _wolfy87Eventemitter = __webpack_require__(2);

	var _wolfy87Eventemitter2 = _interopRequireDefault(_wolfy87Eventemitter);

	var _superagent = __webpack_require__(3);

	var _superagent2 = _interopRequireDefault(_superagent);

	/* 
	 * Downtime monitor service
	 * 
	 * Expects a dowtime API providing this https://github.com/madrobby/downtime
	 */

	var DowntimeMonitor = (function (_EventEmitter) {
		function DowntimeMonitor(downtimeApi, checkInterval) {
			_classCallCheck(this, DowntimeMonitor);

			_get(Object.getPrototypeOf(DowntimeMonitor.prototype), 'constructor', this).call(this);

			this.downtimeApi = downtimeApi || null;
			this.checkInterval = checkInterval || 30000;
			this.downtimes = [];
			this.status = 'up';

			if (!this.downtimeApi) throw new Error('DowntimeMonitorService requires a that you specify downtimeApi.');
			this.start();
		}

		_inherits(DowntimeMonitor, _EventEmitter);

		_createClass(DowntimeMonitor, [{
			key: 'handleRequestError',

			/*
	   * Handle errors from hitting the dowtime API
	   */
			value: function handleRequestError(error) {
				if (error.status === 404) {
					if (console.warn) console.warn('DowntimeMonitorService cannot reach downtime API.');else throw new Error('DowntimeMonitorService cannot reach downtime API.');
				} else if (error.status == 503) {}
			}
		}, {
			key: 'restoreFromCache',

			/*
	   * Retrieve dowtimes from cache
	   */
			value: function restoreFromCache() {
				var downtimesString = localStorage.getItem('downtimes');
				if (downtimesString) return JSON.parse(downtimesString);else return null;
			}
		}, {
			key: 'saveToCache',

			/*
	   * Save downtimes to cache
	   */
			value: function saveToCache(downtimes) {
				if (window.localStorage) {
					localStorage.setItem('downtimes', JSON.stringify(downtimes));
				}
			}
		}, {
			key: 'queryDowntimeApi',

			/*
	   * Check the api
	   */
			value: function queryDowntimeApi() {
				var _this = this;

				_superagent2['default'].get(this.downtimeApi, function (error, res) {
					if (error) return _this.handleRequestError(error);
					var downtime = JSON.parse(res.text);

					_this.downtimes = downtime.downtime;
					_this.reviewDowntimes(_this.downtimes);
				});
			}
		}, {
			key: 'reviewDowntimes',

			/*
	   * Look through updated downtimes and see if we need to emit an event
	   */
			value: function reviewDowntimes(downtimes) {
				var _this2 = this;

				downtimes.forEach(function (downtime) {
					var starts = Date.parse(downtime.startsAt),
					    ends = Date.parse(downtime.endsAt),
					    now = Date.now();

					// Emit downtime now
					if (starts <= now && ends > now) {
						_this2.status = 'down';
						_this2.emit('start-downtime', downtime);

						// Schedule an event for end
						var when = ends - now;
						setTimeout(function () {
							_this2.emit('end-downtime', downtime);
							_this2.status = 'up';
						}, when);
					}
					// Emit downtime in future
					else if (starts > now) {
						var whenStart = starts - now,
						    whenEnds = ends - now;
						// Schedule start event
						setTimeout(function () {
							_this2.status = 'down';
							_this2.emit('start-downtime', downtime);
						}, whenStart);
						// Schedule end event
						setTimeout(function () {
							_this2.emit('end-downtime', downtime);
							_this2.status = 'up';
						}, whenEnds);
					}
				});
			}
		}, {
			key: 'start',

			/*
	   * Start monitoring the downtime API
	   */
			value: function start() {
				// Restore from cache
				// TODO

				this.queryDowntimeApi();

				// Experimental - queue API requests
				// TODO
			}
		}]);

		return DowntimeMonitor;
	})(_wolfy87Eventemitter2['default']);

	window.DowntimeMonitor = DowntimeMonitor;
	exports['default'] = DowntimeMonitor;
	module.exports = exports['default'];

	// Downtime service unavailable

/***/ },
/* 2 */
/***/ function(module, exports, __webpack_require__) {

	var __WEBPACK_AMD_DEFINE_RESULT__;/*!
	 * EventEmitter v4.2.11 - git.io/ee
	 * Unlicense - http://unlicense.org/
	 * Oliver Caldwell - http://oli.me.uk/
	 * @preserve
	 */

	;(function () {
	    'use strict';

	    /**
	     * Class for managing events.
	     * Can be extended to provide event functionality in other classes.
	     *
	     * @class EventEmitter Manages event registering and emitting.
	     */
	    function EventEmitter() {}

	    // Shortcuts to improve speed and size
	    var proto = EventEmitter.prototype;
	    var exports = this;
	    var originalGlobalValue = exports.EventEmitter;

	    /**
	     * Finds the index of the listener for the event in its storage array.
	     *
	     * @param {Function[]} listeners Array of listeners to search through.
	     * @param {Function} listener Method to look for.
	     * @return {Number} Index of the specified listener, -1 if not found
	     * @api private
	     */
	    function indexOfListener(listeners, listener) {
	        var i = listeners.length;
	        while (i--) {
	            if (listeners[i].listener === listener) {
	                return i;
	            }
	        }

	        return -1;
	    }

	    /**
	     * Alias a method while keeping the context correct, to allow for overwriting of target method.
	     *
	     * @param {String} name The name of the target method.
	     * @return {Function} The aliased method
	     * @api private
	     */
	    function alias(name) {
	        return function aliasClosure() {
	            return this[name].apply(this, arguments);
	        };
	    }

	    /**
	     * Returns the listener array for the specified event.
	     * Will initialise the event object and listener arrays if required.
	     * Will return an object if you use a regex search. The object contains keys for each matched event. So /ba[rz]/ might return an object containing bar and baz. But only if you have either defined them with defineEvent or added some listeners to them.
	     * Each property in the object response is an array of listener functions.
	     *
	     * @param {String|RegExp} evt Name of the event to return the listeners from.
	     * @return {Function[]|Object} All listener functions for the event.
	     */
	    proto.getListeners = function getListeners(evt) {
	        var events = this._getEvents();
	        var response;
	        var key;

	        // Return a concatenated array of all matching events if
	        // the selector is a regular expression.
	        if (evt instanceof RegExp) {
	            response = {};
	            for (key in events) {
	                if (events.hasOwnProperty(key) && evt.test(key)) {
	                    response[key] = events[key];
	                }
	            }
	        }
	        else {
	            response = events[evt] || (events[evt] = []);
	        }

	        return response;
	    };

	    /**
	     * Takes a list of listener objects and flattens it into a list of listener functions.
	     *
	     * @param {Object[]} listeners Raw listener objects.
	     * @return {Function[]} Just the listener functions.
	     */
	    proto.flattenListeners = function flattenListeners(listeners) {
	        var flatListeners = [];
	        var i;

	        for (i = 0; i < listeners.length; i += 1) {
	            flatListeners.push(listeners[i].listener);
	        }

	        return flatListeners;
	    };

	    /**
	     * Fetches the requested listeners via getListeners but will always return the results inside an object. This is mainly for internal use but others may find it useful.
	     *
	     * @param {String|RegExp} evt Name of the event to return the listeners from.
	     * @return {Object} All listener functions for an event in an object.
	     */
	    proto.getListenersAsObject = function getListenersAsObject(evt) {
	        var listeners = this.getListeners(evt);
	        var response;

	        if (listeners instanceof Array) {
	            response = {};
	            response[evt] = listeners;
	        }

	        return response || listeners;
	    };

	    /**
	     * Adds a listener function to the specified event.
	     * The listener will not be added if it is a duplicate.
	     * If the listener returns true then it will be removed after it is called.
	     * If you pass a regular expression as the event name then the listener will be added to all events that match it.
	     *
	     * @param {String|RegExp} evt Name of the event to attach the listener to.
	     * @param {Function} listener Method to be called when the event is emitted. If the function returns true then it will be removed after calling.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.addListener = function addListener(evt, listener) {
	        var listeners = this.getListenersAsObject(evt);
	        var listenerIsWrapped = typeof listener === 'object';
	        var key;

	        for (key in listeners) {
	            if (listeners.hasOwnProperty(key) && indexOfListener(listeners[key], listener) === -1) {
	                listeners[key].push(listenerIsWrapped ? listener : {
	                    listener: listener,
	                    once: false
	                });
	            }
	        }

	        return this;
	    };

	    /**
	     * Alias of addListener
	     */
	    proto.on = alias('addListener');

	    /**
	     * Semi-alias of addListener. It will add a listener that will be
	     * automatically removed after its first execution.
	     *
	     * @param {String|RegExp} evt Name of the event to attach the listener to.
	     * @param {Function} listener Method to be called when the event is emitted. If the function returns true then it will be removed after calling.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.addOnceListener = function addOnceListener(evt, listener) {
	        return this.addListener(evt, {
	            listener: listener,
	            once: true
	        });
	    };

	    /**
	     * Alias of addOnceListener.
	     */
	    proto.once = alias('addOnceListener');

	    /**
	     * Defines an event name. This is required if you want to use a regex to add a listener to multiple events at once. If you don't do this then how do you expect it to know what event to add to? Should it just add to every possible match for a regex? No. That is scary and bad.
	     * You need to tell it what event names should be matched by a regex.
	     *
	     * @param {String} evt Name of the event to create.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.defineEvent = function defineEvent(evt) {
	        this.getListeners(evt);
	        return this;
	    };

	    /**
	     * Uses defineEvent to define multiple events.
	     *
	     * @param {String[]} evts An array of event names to define.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.defineEvents = function defineEvents(evts) {
	        for (var i = 0; i < evts.length; i += 1) {
	            this.defineEvent(evts[i]);
	        }
	        return this;
	    };

	    /**
	     * Removes a listener function from the specified event.
	     * When passed a regular expression as the event name, it will remove the listener from all events that match it.
	     *
	     * @param {String|RegExp} evt Name of the event to remove the listener from.
	     * @param {Function} listener Method to remove from the event.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.removeListener = function removeListener(evt, listener) {
	        var listeners = this.getListenersAsObject(evt);
	        var index;
	        var key;

	        for (key in listeners) {
	            if (listeners.hasOwnProperty(key)) {
	                index = indexOfListener(listeners[key], listener);

	                if (index !== -1) {
	                    listeners[key].splice(index, 1);
	                }
	            }
	        }

	        return this;
	    };

	    /**
	     * Alias of removeListener
	     */
	    proto.off = alias('removeListener');

	    /**
	     * Adds listeners in bulk using the manipulateListeners method.
	     * If you pass an object as the second argument you can add to multiple events at once. The object should contain key value pairs of events and listeners or listener arrays. You can also pass it an event name and an array of listeners to be added.
	     * You can also pass it a regular expression to add the array of listeners to all events that match it.
	     * Yeah, this function does quite a bit. That's probably a bad thing.
	     *
	     * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to add to multiple events at once.
	     * @param {Function[]} [listeners] An optional array of listener functions to add.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.addListeners = function addListeners(evt, listeners) {
	        // Pass through to manipulateListeners
	        return this.manipulateListeners(false, evt, listeners);
	    };

	    /**
	     * Removes listeners in bulk using the manipulateListeners method.
	     * If you pass an object as the second argument you can remove from multiple events at once. The object should contain key value pairs of events and listeners or listener arrays.
	     * You can also pass it an event name and an array of listeners to be removed.
	     * You can also pass it a regular expression to remove the listeners from all events that match it.
	     *
	     * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to remove from multiple events at once.
	     * @param {Function[]} [listeners] An optional array of listener functions to remove.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.removeListeners = function removeListeners(evt, listeners) {
	        // Pass through to manipulateListeners
	        return this.manipulateListeners(true, evt, listeners);
	    };

	    /**
	     * Edits listeners in bulk. The addListeners and removeListeners methods both use this to do their job. You should really use those instead, this is a little lower level.
	     * The first argument will determine if the listeners are removed (true) or added (false).
	     * If you pass an object as the second argument you can add/remove from multiple events at once. The object should contain key value pairs of events and listeners or listener arrays.
	     * You can also pass it an event name and an array of listeners to be added/removed.
	     * You can also pass it a regular expression to manipulate the listeners of all events that match it.
	     *
	     * @param {Boolean} remove True if you want to remove listeners, false if you want to add.
	     * @param {String|Object|RegExp} evt An event name if you will pass an array of listeners next. An object if you wish to add/remove from multiple events at once.
	     * @param {Function[]} [listeners] An optional array of listener functions to add/remove.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.manipulateListeners = function manipulateListeners(remove, evt, listeners) {
	        var i;
	        var value;
	        var single = remove ? this.removeListener : this.addListener;
	        var multiple = remove ? this.removeListeners : this.addListeners;

	        // If evt is an object then pass each of its properties to this method
	        if (typeof evt === 'object' && !(evt instanceof RegExp)) {
	            for (i in evt) {
	                if (evt.hasOwnProperty(i) && (value = evt[i])) {
	                    // Pass the single listener straight through to the singular method
	                    if (typeof value === 'function') {
	                        single.call(this, i, value);
	                    }
	                    else {
	                        // Otherwise pass back to the multiple function
	                        multiple.call(this, i, value);
	                    }
	                }
	            }
	        }
	        else {
	            // So evt must be a string
	            // And listeners must be an array of listeners
	            // Loop over it and pass each one to the multiple method
	            i = listeners.length;
	            while (i--) {
	                single.call(this, evt, listeners[i]);
	            }
	        }

	        return this;
	    };

	    /**
	     * Removes all listeners from a specified event.
	     * If you do not specify an event then all listeners will be removed.
	     * That means every event will be emptied.
	     * You can also pass a regex to remove all events that match it.
	     *
	     * @param {String|RegExp} [evt] Optional name of the event to remove all listeners for. Will remove from every event if not passed.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.removeEvent = function removeEvent(evt) {
	        var type = typeof evt;
	        var events = this._getEvents();
	        var key;

	        // Remove different things depending on the state of evt
	        if (type === 'string') {
	            // Remove all listeners for the specified event
	            delete events[evt];
	        }
	        else if (evt instanceof RegExp) {
	            // Remove all events matching the regex.
	            for (key in events) {
	                if (events.hasOwnProperty(key) && evt.test(key)) {
	                    delete events[key];
	                }
	            }
	        }
	        else {
	            // Remove all listeners in all events
	            delete this._events;
	        }

	        return this;
	    };

	    /**
	     * Alias of removeEvent.
	     *
	     * Added to mirror the node API.
	     */
	    proto.removeAllListeners = alias('removeEvent');

	    /**
	     * Emits an event of your choice.
	     * When emitted, every listener attached to that event will be executed.
	     * If you pass the optional argument array then those arguments will be passed to every listener upon execution.
	     * Because it uses `apply`, your array of arguments will be passed as if you wrote them out separately.
	     * So they will not arrive within the array on the other side, they will be separate.
	     * You can also pass a regular expression to emit to all events that match it.
	     *
	     * @param {String|RegExp} evt Name of the event to emit and execute listeners for.
	     * @param {Array} [args] Optional array of arguments to be passed to each listener.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.emitEvent = function emitEvent(evt, args) {
	        var listeners = this.getListenersAsObject(evt);
	        var listener;
	        var i;
	        var key;
	        var response;

	        for (key in listeners) {
	            if (listeners.hasOwnProperty(key)) {
	                i = listeners[key].length;

	                while (i--) {
	                    // If the listener returns true then it shall be removed from the event
	                    // The function is executed either with a basic call or an apply if there is an args array
	                    listener = listeners[key][i];

	                    if (listener.once === true) {
	                        this.removeListener(evt, listener.listener);
	                    }

	                    response = listener.listener.apply(this, args || []);

	                    if (response === this._getOnceReturnValue()) {
	                        this.removeListener(evt, listener.listener);
	                    }
	                }
	            }
	        }

	        return this;
	    };

	    /**
	     * Alias of emitEvent
	     */
	    proto.trigger = alias('emitEvent');

	    /**
	     * Subtly different from emitEvent in that it will pass its arguments on to the listeners, as opposed to taking a single array of arguments to pass on.
	     * As with emitEvent, you can pass a regex in place of the event name to emit to all events that match it.
	     *
	     * @param {String|RegExp} evt Name of the event to emit and execute listeners for.
	     * @param {...*} Optional additional arguments to be passed to each listener.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.emit = function emit(evt) {
	        var args = Array.prototype.slice.call(arguments, 1);
	        return this.emitEvent(evt, args);
	    };

	    /**
	     * Sets the current value to check against when executing listeners. If a
	     * listeners return value matches the one set here then it will be removed
	     * after execution. This value defaults to true.
	     *
	     * @param {*} value The new value to check for when executing listeners.
	     * @return {Object} Current instance of EventEmitter for chaining.
	     */
	    proto.setOnceReturnValue = function setOnceReturnValue(value) {
	        this._onceReturnValue = value;
	        return this;
	    };

	    /**
	     * Fetches the current value to check against when executing listeners. If
	     * the listeners return value matches this one then it should be removed
	     * automatically. It will return true by default.
	     *
	     * @return {*|Boolean} The current value to check for or the default, true.
	     * @api private
	     */
	    proto._getOnceReturnValue = function _getOnceReturnValue() {
	        if (this.hasOwnProperty('_onceReturnValue')) {
	            return this._onceReturnValue;
	        }
	        else {
	            return true;
	        }
	    };

	    /**
	     * Fetches the events object and creates one if required.
	     *
	     * @return {Object} The events storage object.
	     * @api private
	     */
	    proto._getEvents = function _getEvents() {
	        return this._events || (this._events = {});
	    };

	    /**
	     * Reverts the global {@link EventEmitter} to its previous value and returns a reference to this version.
	     *
	     * @return {Function} Non conflicting EventEmitter class.
	     */
	    EventEmitter.noConflict = function noConflict() {
	        exports.EventEmitter = originalGlobalValue;
	        return EventEmitter;
	    };

	    // Expose the class either via AMD, CommonJS or the global object
	    if (true) {
	        !(__WEBPACK_AMD_DEFINE_RESULT__ = function () {
	            return EventEmitter;
	        }.call(exports, __webpack_require__, exports, module), __WEBPACK_AMD_DEFINE_RESULT__ !== undefined && (module.exports = __WEBPACK_AMD_DEFINE_RESULT__));
	    }
	    else if (typeof module === 'object' && module.exports){
	        module.exports = EventEmitter;
	    }
	    else {
	        exports.EventEmitter = EventEmitter;
	    }
	}.call(this));


/***/ },
/* 3 */
/***/ function(module, exports, __webpack_require__) {

	/**
	 * Module dependencies.
	 */

	var Emitter = __webpack_require__(4);
	var reduce = __webpack_require__(5);

	/**
	 * Root reference for iframes.
	 */

	var root = 'undefined' == typeof window
	  ? (this || self)
	  : window;

	/**
	 * Noop.
	 */

	function noop(){};

	/**
	 * Check if `obj` is a host object,
	 * we don't want to serialize these :)
	 *
	 * TODO: future proof, move to compoent land
	 *
	 * @param {Object} obj
	 * @return {Boolean}
	 * @api private
	 */

	function isHost(obj) {
	  var str = {}.toString.call(obj);

	  switch (str) {
	    case '[object File]':
	    case '[object Blob]':
	    case '[object FormData]':
	      return true;
	    default:
	      return false;
	  }
	}

	/**
	 * Determine XHR.
	 */

	request.getXHR = function () {
	  if (root.XMLHttpRequest
	      && (!root.location || 'file:' != root.location.protocol
	          || !root.ActiveXObject)) {
	    return new XMLHttpRequest;
	  } else {
	    try { return new ActiveXObject('Microsoft.XMLHTTP'); } catch(e) {}
	    try { return new ActiveXObject('Msxml2.XMLHTTP.6.0'); } catch(e) {}
	    try { return new ActiveXObject('Msxml2.XMLHTTP.3.0'); } catch(e) {}
	    try { return new ActiveXObject('Msxml2.XMLHTTP'); } catch(e) {}
	  }
	  return false;
	};

	/**
	 * Removes leading and trailing whitespace, added to support IE.
	 *
	 * @param {String} s
	 * @return {String}
	 * @api private
	 */

	var trim = ''.trim
	  ? function(s) { return s.trim(); }
	  : function(s) { return s.replace(/(^\s*|\s*$)/g, ''); };

	/**
	 * Check if `obj` is an object.
	 *
	 * @param {Object} obj
	 * @return {Boolean}
	 * @api private
	 */

	function isObject(obj) {
	  return obj === Object(obj);
	}

	/**
	 * Serialize the given `obj`.
	 *
	 * @param {Object} obj
	 * @return {String}
	 * @api private
	 */

	function serialize(obj) {
	  if (!isObject(obj)) return obj;
	  var pairs = [];
	  for (var key in obj) {
	    if (null != obj[key]) {
	      pairs.push(encodeURIComponent(key)
	        + '=' + encodeURIComponent(obj[key]));
	    }
	  }
	  return pairs.join('&');
	}

	/**
	 * Expose serialization method.
	 */

	 request.serializeObject = serialize;

	 /**
	  * Parse the given x-www-form-urlencoded `str`.
	  *
	  * @param {String} str
	  * @return {Object}
	  * @api private
	  */

	function parseString(str) {
	  var obj = {};
	  var pairs = str.split('&');
	  var parts;
	  var pair;

	  for (var i = 0, len = pairs.length; i < len; ++i) {
	    pair = pairs[i];
	    parts = pair.split('=');
	    obj[decodeURIComponent(parts[0])] = decodeURIComponent(parts[1]);
	  }

	  return obj;
	}

	/**
	 * Expose parser.
	 */

	request.parseString = parseString;

	/**
	 * Default MIME type map.
	 *
	 *     superagent.types.xml = 'application/xml';
	 *
	 */

	request.types = {
	  html: 'text/html',
	  json: 'application/json',
	  xml: 'application/xml',
	  urlencoded: 'application/x-www-form-urlencoded',
	  'form': 'application/x-www-form-urlencoded',
	  'form-data': 'application/x-www-form-urlencoded'
	};

	/**
	 * Default serialization map.
	 *
	 *     superagent.serialize['application/xml'] = function(obj){
	 *       return 'generated xml here';
	 *     };
	 *
	 */

	 request.serialize = {
	   'application/x-www-form-urlencoded': serialize,
	   'application/json': JSON.stringify
	 };

	 /**
	  * Default parsers.
	  *
	  *     superagent.parse['application/xml'] = function(str){
	  *       return { object parsed from str };
	  *     };
	  *
	  */

	request.parse = {
	  'application/x-www-form-urlencoded': parseString,
	  'application/json': JSON.parse
	};

	/**
	 * Parse the given header `str` into
	 * an object containing the mapped fields.
	 *
	 * @param {String} str
	 * @return {Object}
	 * @api private
	 */

	function parseHeader(str) {
	  var lines = str.split(/\r?\n/);
	  var fields = {};
	  var index;
	  var line;
	  var field;
	  var val;

	  lines.pop(); // trailing CRLF

	  for (var i = 0, len = lines.length; i < len; ++i) {
	    line = lines[i];
	    index = line.indexOf(':');
	    field = line.slice(0, index).toLowerCase();
	    val = trim(line.slice(index + 1));
	    fields[field] = val;
	  }

	  return fields;
	}

	/**
	 * Return the mime type for the given `str`.
	 *
	 * @param {String} str
	 * @return {String}
	 * @api private
	 */

	function type(str){
	  return str.split(/ *; */).shift();
	};

	/**
	 * Return header field parameters.
	 *
	 * @param {String} str
	 * @return {Object}
	 * @api private
	 */

	function params(str){
	  return reduce(str.split(/ *; */), function(obj, str){
	    var parts = str.split(/ *= */)
	      , key = parts.shift()
	      , val = parts.shift();

	    if (key && val) obj[key] = val;
	    return obj;
	  }, {});
	};

	/**
	 * Initialize a new `Response` with the given `xhr`.
	 *
	 *  - set flags (.ok, .error, etc)
	 *  - parse header
	 *
	 * Examples:
	 *
	 *  Aliasing `superagent` as `request` is nice:
	 *
	 *      request = superagent;
	 *
	 *  We can use the promise-like API, or pass callbacks:
	 *
	 *      request.get('/').end(function(res){});
	 *      request.get('/', function(res){});
	 *
	 *  Sending data can be chained:
	 *
	 *      request
	 *        .post('/user')
	 *        .send({ name: 'tj' })
	 *        .end(function(res){});
	 *
	 *  Or passed to `.send()`:
	 *
	 *      request
	 *        .post('/user')
	 *        .send({ name: 'tj' }, function(res){});
	 *
	 *  Or passed to `.post()`:
	 *
	 *      request
	 *        .post('/user', { name: 'tj' })
	 *        .end(function(res){});
	 *
	 * Or further reduced to a single call for simple cases:
	 *
	 *      request
	 *        .post('/user', { name: 'tj' }, function(res){});
	 *
	 * @param {XMLHTTPRequest} xhr
	 * @param {Object} options
	 * @api private
	 */

	function Response(req, options) {
	  options = options || {};
	  this.req = req;
	  this.xhr = this.req.xhr;
	  // responseText is accessible only if responseType is '' or 'text' and on older browsers
	  this.text = ((this.req.method !='HEAD' && (this.xhr.responseType === '' || this.xhr.responseType === 'text')) || typeof this.xhr.responseType === 'undefined')
	     ? this.xhr.responseText
	     : null;
	  this.statusText = this.req.xhr.statusText;
	  this.setStatusProperties(this.xhr.status);
	  this.header = this.headers = parseHeader(this.xhr.getAllResponseHeaders());
	  // getAllResponseHeaders sometimes falsely returns "" for CORS requests, but
	  // getResponseHeader still works. so we get content-type even if getting
	  // other headers fails.
	  this.header['content-type'] = this.xhr.getResponseHeader('content-type');
	  this.setHeaderProperties(this.header);
	  this.body = this.req.method != 'HEAD'
	    ? this.parseBody(this.text ? this.text : this.xhr.response)
	    : null;
	}

	/**
	 * Get case-insensitive `field` value.
	 *
	 * @param {String} field
	 * @return {String}
	 * @api public
	 */

	Response.prototype.get = function(field){
	  return this.header[field.toLowerCase()];
	};

	/**
	 * Set header related properties:
	 *
	 *   - `.type` the content type without params
	 *
	 * A response of "Content-Type: text/plain; charset=utf-8"
	 * will provide you with a `.type` of "text/plain".
	 *
	 * @param {Object} header
	 * @api private
	 */

	Response.prototype.setHeaderProperties = function(header){
	  // content-type
	  var ct = this.header['content-type'] || '';
	  this.type = type(ct);

	  // params
	  var obj = params(ct);
	  for (var key in obj) this[key] = obj[key];
	};

	/**
	 * Parse the given body `str`.
	 *
	 * Used for auto-parsing of bodies. Parsers
	 * are defined on the `superagent.parse` object.
	 *
	 * @param {String} str
	 * @return {Mixed}
	 * @api private
	 */

	Response.prototype.parseBody = function(str){
	  var parse = request.parse[this.type];
	  return parse && str && (str.length || str instanceof Object)
	    ? parse(str)
	    : null;
	};

	/**
	 * Set flags such as `.ok` based on `status`.
	 *
	 * For example a 2xx response will give you a `.ok` of __true__
	 * whereas 5xx will be __false__ and `.error` will be __true__. The
	 * `.clientError` and `.serverError` are also available to be more
	 * specific, and `.statusType` is the class of error ranging from 1..5
	 * sometimes useful for mapping respond colors etc.
	 *
	 * "sugar" properties are also defined for common cases. Currently providing:
	 *
	 *   - .noContent
	 *   - .badRequest
	 *   - .unauthorized
	 *   - .notAcceptable
	 *   - .notFound
	 *
	 * @param {Number} status
	 * @api private
	 */

	Response.prototype.setStatusProperties = function(status){
	  // handle IE9 bug: http://stackoverflow.com/questions/10046972/msie-returns-status-code-of-1223-for-ajax-request
	  if (status === 1223) {
	    status = 204;
	  }

	  var type = status / 100 | 0;

	  // status / class
	  this.status = status;
	  this.statusType = type;

	  // basics
	  this.info = 1 == type;
	  this.ok = 2 == type;
	  this.clientError = 4 == type;
	  this.serverError = 5 == type;
	  this.error = (4 == type || 5 == type)
	    ? this.toError()
	    : false;

	  // sugar
	  this.accepted = 202 == status;
	  this.noContent = 204 == status;
	  this.badRequest = 400 == status;
	  this.unauthorized = 401 == status;
	  this.notAcceptable = 406 == status;
	  this.notFound = 404 == status;
	  this.forbidden = 403 == status;
	};

	/**
	 * Return an `Error` representative of this response.
	 *
	 * @return {Error}
	 * @api public
	 */

	Response.prototype.toError = function(){
	  var req = this.req;
	  var method = req.method;
	  var url = req.url;

	  var msg = 'cannot ' + method + ' ' + url + ' (' + this.status + ')';
	  var err = new Error(msg);
	  err.status = this.status;
	  err.method = method;
	  err.url = url;

	  return err;
	};

	/**
	 * Expose `Response`.
	 */

	request.Response = Response;

	/**
	 * Initialize a new `Request` with the given `method` and `url`.
	 *
	 * @param {String} method
	 * @param {String} url
	 * @api public
	 */

	function Request(method, url) {
	  var self = this;
	  Emitter.call(this);
	  this._query = this._query || [];
	  this.method = method;
	  this.url = url;
	  this.header = {};
	  this._header = {};
	  this.on('end', function(){
	    var err = null;
	    var res = null;

	    try {
	      res = new Response(self);
	    } catch(e) {
	      err = new Error('Parser is unable to parse the response');
	      err.parse = true;
	      err.original = e;
	      return self.callback(err);
	    }

	    self.emit('response', res);

	    if (err) {
	      return self.callback(err, res);
	    }

	    if (res.status >= 200 && res.status < 300) {
	      return self.callback(err, res);
	    }

	    var new_err = new Error(res.statusText || 'Unsuccessful HTTP response');
	    new_err.original = err;
	    new_err.response = res;
	    new_err.status = res.status;

	    self.callback(err || new_err, res);
	  });
	}

	/**
	 * Mixin `Emitter`.
	 */

	Emitter(Request.prototype);

	/**
	 * Allow for extension
	 */

	Request.prototype.use = function(fn) {
	  fn(this);
	  return this;
	}

	/**
	 * Set timeout to `ms`.
	 *
	 * @param {Number} ms
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.timeout = function(ms){
	  this._timeout = ms;
	  return this;
	};

	/**
	 * Clear previous timeout.
	 *
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.clearTimeout = function(){
	  this._timeout = 0;
	  clearTimeout(this._timer);
	  return this;
	};

	/**
	 * Abort the request, and clear potential timeout.
	 *
	 * @return {Request}
	 * @api public
	 */

	Request.prototype.abort = function(){
	  if (this.aborted) return;
	  this.aborted = true;
	  this.xhr.abort();
	  this.clearTimeout();
	  this.emit('abort');
	  return this;
	};

	/**
	 * Set header `field` to `val`, or multiple fields with one object.
	 *
	 * Examples:
	 *
	 *      req.get('/')
	 *        .set('Accept', 'application/json')
	 *        .set('X-API-Key', 'foobar')
	 *        .end(callback);
	 *
	 *      req.get('/')
	 *        .set({ Accept: 'application/json', 'X-API-Key': 'foobar' })
	 *        .end(callback);
	 *
	 * @param {String|Object} field
	 * @param {String} val
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.set = function(field, val){
	  if (isObject(field)) {
	    for (var key in field) {
	      this.set(key, field[key]);
	    }
	    return this;
	  }
	  this._header[field.toLowerCase()] = val;
	  this.header[field] = val;
	  return this;
	};

	/**
	 * Remove header `field`.
	 *
	 * Example:
	 *
	 *      req.get('/')
	 *        .unset('User-Agent')
	 *        .end(callback);
	 *
	 * @param {String} field
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.unset = function(field){
	  delete this._header[field.toLowerCase()];
	  delete this.header[field];
	  return this;
	};

	/**
	 * Get case-insensitive header `field` value.
	 *
	 * @param {String} field
	 * @return {String}
	 * @api private
	 */

	Request.prototype.getHeader = function(field){
	  return this._header[field.toLowerCase()];
	};

	/**
	 * Set Content-Type to `type`, mapping values from `request.types`.
	 *
	 * Examples:
	 *
	 *      superagent.types.xml = 'application/xml';
	 *
	 *      request.post('/')
	 *        .type('xml')
	 *        .send(xmlstring)
	 *        .end(callback);
	 *
	 *      request.post('/')
	 *        .type('application/xml')
	 *        .send(xmlstring)
	 *        .end(callback);
	 *
	 * @param {String} type
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.type = function(type){
	  this.set('Content-Type', request.types[type] || type);
	  return this;
	};

	/**
	 * Set Accept to `type`, mapping values from `request.types`.
	 *
	 * Examples:
	 *
	 *      superagent.types.json = 'application/json';
	 *
	 *      request.get('/agent')
	 *        .accept('json')
	 *        .end(callback);
	 *
	 *      request.get('/agent')
	 *        .accept('application/json')
	 *        .end(callback);
	 *
	 * @param {String} accept
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.accept = function(type){
	  this.set('Accept', request.types[type] || type);
	  return this;
	};

	/**
	 * Set Authorization field value with `user` and `pass`.
	 *
	 * @param {String} user
	 * @param {String} pass
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.auth = function(user, pass){
	  var str = btoa(user + ':' + pass);
	  this.set('Authorization', 'Basic ' + str);
	  return this;
	};

	/**
	* Add query-string `val`.
	*
	* Examples:
	*
	*   request.get('/shoes')
	*     .query('size=10')
	*     .query({ color: 'blue' })
	*
	* @param {Object|String} val
	* @return {Request} for chaining
	* @api public
	*/

	Request.prototype.query = function(val){
	  if ('string' != typeof val) val = serialize(val);
	  if (val) this._query.push(val);
	  return this;
	};

	/**
	 * Write the field `name` and `val` for "multipart/form-data"
	 * request bodies.
	 *
	 * ``` js
	 * request.post('/upload')
	 *   .field('foo', 'bar')
	 *   .end(callback);
	 * ```
	 *
	 * @param {String} name
	 * @param {String|Blob|File} val
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.field = function(name, val){
	  if (!this._formData) this._formData = new root.FormData();
	  this._formData.append(name, val);
	  return this;
	};

	/**
	 * Queue the given `file` as an attachment to the specified `field`,
	 * with optional `filename`.
	 *
	 * ``` js
	 * request.post('/upload')
	 *   .attach(new Blob(['<a id="a"><b id="b">hey!</b></a>'], { type: "text/html"}))
	 *   .end(callback);
	 * ```
	 *
	 * @param {String} field
	 * @param {Blob|File} file
	 * @param {String} filename
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.attach = function(field, file, filename){
	  if (!this._formData) this._formData = new root.FormData();
	  this._formData.append(field, file, filename);
	  return this;
	};

	/**
	 * Send `data`, defaulting the `.type()` to "json" when
	 * an object is given.
	 *
	 * Examples:
	 *
	 *       // querystring
	 *       request.get('/search')
	 *         .end(callback)
	 *
	 *       // multiple data "writes"
	 *       request.get('/search')
	 *         .send({ search: 'query' })
	 *         .send({ range: '1..5' })
	 *         .send({ order: 'desc' })
	 *         .end(callback)
	 *
	 *       // manual json
	 *       request.post('/user')
	 *         .type('json')
	 *         .send('{"name":"tj"})
	 *         .end(callback)
	 *
	 *       // auto json
	 *       request.post('/user')
	 *         .send({ name: 'tj' })
	 *         .end(callback)
	 *
	 *       // manual x-www-form-urlencoded
	 *       request.post('/user')
	 *         .type('form')
	 *         .send('name=tj')
	 *         .end(callback)
	 *
	 *       // auto x-www-form-urlencoded
	 *       request.post('/user')
	 *         .type('form')
	 *         .send({ name: 'tj' })
	 *         .end(callback)
	 *
	 *       // defaults to x-www-form-urlencoded
	  *      request.post('/user')
	  *        .send('name=tobi')
	  *        .send('species=ferret')
	  *        .end(callback)
	 *
	 * @param {String|Object} data
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.send = function(data){
	  var obj = isObject(data);
	  var type = this.getHeader('Content-Type');

	  // merge
	  if (obj && isObject(this._data)) {
	    for (var key in data) {
	      this._data[key] = data[key];
	    }
	  } else if ('string' == typeof data) {
	    if (!type) this.type('form');
	    type = this.getHeader('Content-Type');
	    if ('application/x-www-form-urlencoded' == type) {
	      this._data = this._data
	        ? this._data + '&' + data
	        : data;
	    } else {
	      this._data = (this._data || '') + data;
	    }
	  } else {
	    this._data = data;
	  }

	  if (!obj || isHost(data)) return this;
	  if (!type) this.type('json');
	  return this;
	};

	/**
	 * Invoke the callback with `err` and `res`
	 * and handle arity check.
	 *
	 * @param {Error} err
	 * @param {Response} res
	 * @api private
	 */

	Request.prototype.callback = function(err, res){
	  var fn = this._callback;
	  this.clearTimeout();
	  fn(err, res);
	};

	/**
	 * Invoke callback with x-domain error.
	 *
	 * @api private
	 */

	Request.prototype.crossDomainError = function(){
	  var err = new Error('Origin is not allowed by Access-Control-Allow-Origin');
	  err.crossDomain = true;
	  this.callback(err);
	};

	/**
	 * Invoke callback with timeout error.
	 *
	 * @api private
	 */

	Request.prototype.timeoutError = function(){
	  var timeout = this._timeout;
	  var err = new Error('timeout of ' + timeout + 'ms exceeded');
	  err.timeout = timeout;
	  this.callback(err);
	};

	/**
	 * Enable transmission of cookies with x-domain requests.
	 *
	 * Note that for this to work the origin must not be
	 * using "Access-Control-Allow-Origin" with a wildcard,
	 * and also must set "Access-Control-Allow-Credentials"
	 * to "true".
	 *
	 * @api public
	 */

	Request.prototype.withCredentials = function(){
	  this._withCredentials = true;
	  return this;
	};

	/**
	 * Initiate request, invoking callback `fn(res)`
	 * with an instanceof `Response`.
	 *
	 * @param {Function} fn
	 * @return {Request} for chaining
	 * @api public
	 */

	Request.prototype.end = function(fn){
	  var self = this;
	  var xhr = this.xhr = request.getXHR();
	  var query = this._query.join('&');
	  var timeout = this._timeout;
	  var data = this._formData || this._data;

	  // store callback
	  this._callback = fn || noop;

	  // state change
	  xhr.onreadystatechange = function(){
	    if (4 != xhr.readyState) return;

	    // In IE9, reads to any property (e.g. status) off of an aborted XHR will
	    // result in the error "Could not complete the operation due to error c00c023f"
	    var status;
	    try { status = xhr.status } catch(e) { status = 0; }

	    if (0 == status) {
	      if (self.timedout) return self.timeoutError();
	      if (self.aborted) return;
	      return self.crossDomainError();
	    }
	    self.emit('end');
	  };

	  // progress
	  var handleProgress = function(e){
	    if (e.total > 0) {
	      e.percent = e.loaded / e.total * 100;
	    }
	    self.emit('progress', e);
	  };
	  if (this.hasListeners('progress')) {
	    xhr.onprogress = handleProgress;
	  }
	  try {
	    if (xhr.upload && this.hasListeners('progress')) {
	      xhr.upload.onprogress = handleProgress;
	    }
	  } catch(e) {
	    // Accessing xhr.upload fails in IE from a web worker, so just pretend it doesn't exist.
	    // Reported here:
	    // https://connect.microsoft.com/IE/feedback/details/837245/xmlhttprequest-upload-throws-invalid-argument-when-used-from-web-worker-context
	  }

	  // timeout
	  if (timeout && !this._timer) {
	    this._timer = setTimeout(function(){
	      self.timedout = true;
	      self.abort();
	    }, timeout);
	  }

	  // querystring
	  if (query) {
	    query = request.serializeObject(query);
	    this.url += ~this.url.indexOf('?')
	      ? '&' + query
	      : '?' + query;
	  }

	  // initiate request
	  xhr.open(this.method, this.url, true);

	  // CORS
	  if (this._withCredentials) xhr.withCredentials = true;

	  // body
	  if ('GET' != this.method && 'HEAD' != this.method && 'string' != typeof data && !isHost(data)) {
	    // serialize stuff
	    var serialize = request.serialize[this.getHeader('Content-Type')];
	    if (serialize) data = serialize(data);
	  }

	  // set header fields
	  for (var field in this.header) {
	    if (null == this.header[field]) continue;
	    xhr.setRequestHeader(field, this.header[field]);
	  }

	  // send stuff
	  this.emit('request', this);
	  xhr.send(data);
	  return this;
	};

	/**
	 * Expose `Request`.
	 */

	request.Request = Request;

	/**
	 * Issue a request:
	 *
	 * Examples:
	 *
	 *    request('GET', '/users').end(callback)
	 *    request('/users').end(callback)
	 *    request('/users', callback)
	 *
	 * @param {String} method
	 * @param {String|Function} url or callback
	 * @return {Request}
	 * @api public
	 */

	function request(method, url) {
	  // callback
	  if ('function' == typeof url) {
	    return new Request('GET', method).end(url);
	  }

	  // url first
	  if (1 == arguments.length) {
	    return new Request('GET', method);
	  }

	  return new Request(method, url);
	}

	/**
	 * GET `url` with optional callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Mixed|Function} data or fn
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.get = function(url, data, fn){
	  var req = request('GET', url);
	  if ('function' == typeof data) fn = data, data = null;
	  if (data) req.query(data);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * HEAD `url` with optional callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Mixed|Function} data or fn
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.head = function(url, data, fn){
	  var req = request('HEAD', url);
	  if ('function' == typeof data) fn = data, data = null;
	  if (data) req.send(data);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * DELETE `url` with optional callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.del = function(url, fn){
	  var req = request('DELETE', url);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * PATCH `url` with optional `data` and callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Mixed} data
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.patch = function(url, data, fn){
	  var req = request('PATCH', url);
	  if ('function' == typeof data) fn = data, data = null;
	  if (data) req.send(data);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * POST `url` with optional `data` and callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Mixed} data
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.post = function(url, data, fn){
	  var req = request('POST', url);
	  if ('function' == typeof data) fn = data, data = null;
	  if (data) req.send(data);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * PUT `url` with optional `data` and callback `fn(res)`.
	 *
	 * @param {String} url
	 * @param {Mixed|Function} data or fn
	 * @param {Function} fn
	 * @return {Request}
	 * @api public
	 */

	request.put = function(url, data, fn){
	  var req = request('PUT', url);
	  if ('function' == typeof data) fn = data, data = null;
	  if (data) req.send(data);
	  if (fn) req.end(fn);
	  return req;
	};

	/**
	 * Expose `request`.
	 */

	module.exports = request;


/***/ },
/* 4 */
/***/ function(module, exports, __webpack_require__) {

	
	/**
	 * Expose `Emitter`.
	 */

	module.exports = Emitter;

	/**
	 * Initialize a new `Emitter`.
	 *
	 * @api public
	 */

	function Emitter(obj) {
	  if (obj) return mixin(obj);
	};

	/**
	 * Mixin the emitter properties.
	 *
	 * @param {Object} obj
	 * @return {Object}
	 * @api private
	 */

	function mixin(obj) {
	  for (var key in Emitter.prototype) {
	    obj[key] = Emitter.prototype[key];
	  }
	  return obj;
	}

	/**
	 * Listen on the given `event` with `fn`.
	 *
	 * @param {String} event
	 * @param {Function} fn
	 * @return {Emitter}
	 * @api public
	 */

	Emitter.prototype.on =
	Emitter.prototype.addEventListener = function(event, fn){
	  this._callbacks = this._callbacks || {};
	  (this._callbacks[event] = this._callbacks[event] || [])
	    .push(fn);
	  return this;
	};

	/**
	 * Adds an `event` listener that will be invoked a single
	 * time then automatically removed.
	 *
	 * @param {String} event
	 * @param {Function} fn
	 * @return {Emitter}
	 * @api public
	 */

	Emitter.prototype.once = function(event, fn){
	  var self = this;
	  this._callbacks = this._callbacks || {};

	  function on() {
	    self.off(event, on);
	    fn.apply(this, arguments);
	  }

	  on.fn = fn;
	  this.on(event, on);
	  return this;
	};

	/**
	 * Remove the given callback for `event` or all
	 * registered callbacks.
	 *
	 * @param {String} event
	 * @param {Function} fn
	 * @return {Emitter}
	 * @api public
	 */

	Emitter.prototype.off =
	Emitter.prototype.removeListener =
	Emitter.prototype.removeAllListeners =
	Emitter.prototype.removeEventListener = function(event, fn){
	  this._callbacks = this._callbacks || {};

	  // all
	  if (0 == arguments.length) {
	    this._callbacks = {};
	    return this;
	  }

	  // specific event
	  var callbacks = this._callbacks[event];
	  if (!callbacks) return this;

	  // remove all handlers
	  if (1 == arguments.length) {
	    delete this._callbacks[event];
	    return this;
	  }

	  // remove specific handler
	  var cb;
	  for (var i = 0; i < callbacks.length; i++) {
	    cb = callbacks[i];
	    if (cb === fn || cb.fn === fn) {
	      callbacks.splice(i, 1);
	      break;
	    }
	  }
	  return this;
	};

	/**
	 * Emit `event` with the given args.
	 *
	 * @param {String} event
	 * @param {Mixed} ...
	 * @return {Emitter}
	 */

	Emitter.prototype.emit = function(event){
	  this._callbacks = this._callbacks || {};
	  var args = [].slice.call(arguments, 1)
	    , callbacks = this._callbacks[event];

	  if (callbacks) {
	    callbacks = callbacks.slice(0);
	    for (var i = 0, len = callbacks.length; i < len; ++i) {
	      callbacks[i].apply(this, args);
	    }
	  }

	  return this;
	};

	/**
	 * Return array of callbacks for `event`.
	 *
	 * @param {String} event
	 * @return {Array}
	 * @api public
	 */

	Emitter.prototype.listeners = function(event){
	  this._callbacks = this._callbacks || {};
	  return this._callbacks[event] || [];
	};

	/**
	 * Check if this emitter has `event` handlers.
	 *
	 * @param {String} event
	 * @return {Boolean}
	 * @api public
	 */

	Emitter.prototype.hasListeners = function(event){
	  return !! this.listeners(event).length;
	};


/***/ },
/* 5 */
/***/ function(module, exports, __webpack_require__) {

	
	/**
	 * Reduce `arr` with `fn`.
	 *
	 * @param {Array} arr
	 * @param {Function} fn
	 * @param {Mixed} initial
	 *
	 * TODO: combatible error handling?
	 */

	module.exports = function(arr, fn, initial){  
	  var idx = 0;
	  var len = arr.length;
	  var curr = arguments.length == 3
	    ? initial
	    : arr[idx++];

	  while (idx < len) {
	    curr = fn.call(null, curr, arr[idx], ++idx, arr);
	  }
	  
	  return curr;
	};

/***/ }
/******/ ]);
window.Acorns = (window.Acorns || {});
window.Acorns.countries =
[{"name":"Afghanistan","code":"AF"},{"name":"Albania","code":"AL"},{"name":"Algeria","code":"DZ"},{"name":"American Samoa","code":"AS"},{"name":"Andorra","code":"AD"},{"name":"Angola","code":"AO"},{"name":"Anguilla","code":"AI"},{"name":"Argentina","code":"AR"},{"name":"Armenia","code":"AM"},{"name":"Aruba","code":"AW"},{"name":"Australia","code":"AU"},{"name":"Austria","code":"AT"},{"name":"Azerbaijan","code":"AZ"},{"name":"Bahamas","code":"BS"},{"name":"Bahrain","code":"BH"},{"name":"Bangladesh","code":"BD"},{"name":"Barbados","code":"BB"},{"name":"Belarus","code":"BY"},{"name":"Belgium","code":"BE"},{"name":"Belize","code":"BZ"},{"name":"Benin","code":"BJ"},{"name":"Bermuda","code":"BM"},{"name":"Bhutan","code":"BT"},{"name":"Botswana","code":"BW"},{"name":"Brazil","code":"BR"},{"name":"Bulgaria","code":"BG"},{"name":"Burkina Faso","code":"BF"},{"name":"Burundi","code":"BI"},{"name":"Cambodia","code":"KH"},{"name":"Cameroon","code":"CM"},{"name":"Canada","code":"CA"},{"name":"Cape Verde","code":"CV"},{"name":"Cayman Islands","code":"KY"},{"name":"Central African Republic","code":"CF"},{"name":"Chad","code":"TD"},{"name":"Chile","code":"CL"},{"name":"China","code":"CN"},{"name":"Colombia","code":"CO"},{"name":"Comoros","code":"KM"},{"name":"Cook Islands","code":"CK"},{"name":"Costa Rica","code":"CR"},{"name":"Croatia","code":"HR"},{"name":"Cuba","code":"CU"},{"name":"Cyprus","code":"CY"},{"name":"Czech Republic","code":"CZ"},{"name":"Denmark","code":"DK"},{"name":"Djibouti","code":"DJ"},{"name":"Dominica","code":"DM"},{"name":"Dominican Republic","code":"DO"},{"name":"Ecuador","code":"EC"},{"name":"Egypt","code":"EG"},{"name":"El Salvador","code":"SV"},{"name":"Equatorial Guinea","code":"GQ"},{"name":"Eritrea","code":"ER"},{"name":"Estonia","code":"EE"},{"name":"Ethiopia","code":"ET"},{"name":"Fiji","code":"FJ"},{"name":"Finland","code":"FI"},{"name":"France","code":"FR"},{"name":"Gabon","code":"GA"},{"name":"Gambia","code":"GM"},{"name":"Georgia","code":"GE"},{"name":"Germany","code":"DE"},{"name":"Ghana","code":"GH"},{"name":"Gibraltar","code":"GI"},{"name":"Greece","code":"GR"},{"name":"Greenland","code":"GL"},{"name":"Grenada","code":"GD"},{"name":"Guadeloupe","code":"GP"},{"name":"Guam","code":"GU"},{"name":"Guatemala","code":"GT"},{"name":"Guernsey","code":"GG"},{"name":"Guinea","code":"GN"},{"name":"Guinea-Bissau","code":"GW"},{"name":"Guyana","code":"GY"},{"name":"Haiti","code":"HT"},{"name":"Honduras","code":"HN"},{"name":"Hong Kong","code":"HK"},{"name":"Hungary","code":"HU"},{"name":"Iceland","code":"IS"},{"name":"India","code":"IN"},{"name":"Indonesia","code":"ID"},{"name":"Iran","code":"IR"},{"name":"Iraq","code":"IQ"},{"name":"Ireland","code":"IE"},{"name":"Isle of Man","code":"IM"},{"name":"Israel","code":"IL"},{"name":"Italy","code":"IT"},{"name":"Jamaica","code":"JM"},{"name":"Japan","code":"JP"},{"name":"Jersey","code":"JE"},{"name":"Jordan","code":"JO"},{"name":"Kazakhstan","code":"KZ"},{"name":"Kenya","code":"KE"},{"name":"Kiribati","code":"KI"},{"name":"South Korea","code":"KR"},{"name":"Kuwait","code":"KW"},{"name":"Kyrgyzstan","code":"KG"},{"name":"Latvia","code":"LV"},{"name":"Lebanon","code":"LB"},{"name":"Lesotho","code":"LS"},{"name":"Liberia","code":"LR"},{"name":"Libya","code":"LY"},{"name":"Liechtenstein","code":"LI"},{"name":"Luxembourg","code":"LU"},{"name":"Macao","code":"MO"},{"name":"Macedonia","code":"MK"},{"name":"Madagascar","code":"MG"},{"name":"Malawi","code":"MW"},{"name":"Malaysia","code":"MY"},{"name":"Maldives","code":"MV"},{"name":"Mali","code":"ML"},{"name":"Malta","code":"MT"},{"name":"Marshall Islands","code":"MH"},{"name":"Martinique","code":"MQ"},{"name":"Mauritania","code":"MR"},{"name":"Mauritius","code":"MU"},{"name":"Mexico","code":"MX"},{"name":"Micronesia","code":"FM"},{"name":"Moldova","code":"MD"},{"name":"Monaco","code":"MC"},{"name":"Mongolia","code":"MN"},{"name":"Montenegro","code":"ME"},{"name":"Montserrat","code":"MS"},{"name":"Morocco","code":"MA"},{"name":"Mozambique","code":"MZ"},{"name":"Myanmar","code":"MM"},{"name":"Namibia","code":"NA"},{"name":"Nauru","code":"NR"},{"name":"Nepal","code":"NP"},{"name":"Netherlands","code":"NL"},{"name":"New Caledonia","code":"NC"},{"name":"New Zealand","code":"NZ"},{"name":"Nicaragua","code":"NI"},{"name":"Niger","code":"NE"},{"name":"Nigeria","code":"NG"},{"name":"Norway","code":"NO"},{"name":"Oman","code":"OM"},{"name":"Pakistan","code":"PK"},{"name":"Palau","code":"PW"},{"name":"Palestine","code":"PS"},{"name":"Panama","code":"PA"},{"name":"Papua New Guinea","code":"PG"},{"name":"Paraguay","code":"PY"},{"name":"Peru","code":"PE"},{"name":"Philippines","code":"PH"},{"name":"Poland","code":"PL"},{"name":"Portugal","code":"PT"},{"name":"Qatar","code":"QA"},{"name":"Reunion","code":"RE"},{"name":"Romania","code":"RO"},{"name":"Russian Federation","code":"RU"},{"name":"Rwanda","code":"RW"},{"name":"Saint Lucia","code":"LC"},{"name":"Samoa","code":"WS"},{"name":"San Marino","code":"SM"},{"name":"Sao Tome and Principe","code":"ST"},{"name":"Saudi Arabia","code":"SA"},{"name":"Senegal","code":"SN"},{"name":"Serbia","code":"RS"},{"name":"Seychelles","code":"SC"},{"name":"Sierra Leone","code":"SL"},{"name":"Singapore","code":"SG"},{"name":"Slovakia","code":"SK"},{"name":"Slovenia","code":"SI"},{"name":"Solomon Islands","code":"SB"},{"name":"Somalia","code":"SO"},{"name":"South Africa","code":"ZA"},{"name":"Sudan","code":"SD"},{"name":"Spain","code":"ES"},{"name":"Sri Lanka","code":"LK"},{"name":"Sudan","code":"SD"},{"name":"Suriname","code":"SR"},{"name":"Swaziland","code":"SZ"},{"name":"Sweden","code":"SE"},{"name":"Switzerland","code":"CH"},{"name":"Syria","code":"SY"},{"name":"Taiwan","code":"TW"},{"name":"Tajikistan","code":"TJ"},{"name":"Tanzania","code":"TZ"},{"name":"Thailand","code":"TH"},{"name":"Timor-Leste","code":"TL"},{"name":"Togo","code":"TG"},{"name":"Tonga","code":"TO"},{"name":"Trinidad and Tobago","code":"TT"},{"name":"Tunisia","code":"TN"},{"name":"Turkey","code":"TR"},{"name":"Turkmenistan","code":"TM"},{"name":"Turks and Caicos Islands","code":"TC"},{"name":"Tuvalu","code":"TV"},{"name":"Uganda","code":"UG"},{"name":"Ukraine","code":"UA"},{"name":"United Arab Emirates","code":"AE"},{"name":"United Kingdom","code":"GB"},{"name":"Uruguay","code":"UY"},{"name":"Uzbekistan","code":"UZ"},{"name":"Venezuela","code":"VE"},{"name":"Viet Nam","code":"VN"},{"name":"Western Sahara","code":"EH"},{"name":"Yemen","code":"YE"},{"name":"Zambia","code":"ZM"},{"name":"Zimbabwe","code":"ZW"}]
$(function() {
  'use strict';
  var globalSection =  $('.global');
  var clicked = false;
  $('#subscribe-email-button').on('click', function(e) {
    e.preventDefault();

    // Ignore first click
    if (!clicked) {
      clicked = true;
      return;
    };

    var email = $('#email-input').val(),
      isRealEmail = validateEmail(email),

      $this = $(e.currentTarget),
      formIsGood;
    if (window.Acorns.countrySelected == 'Australia') {
      // Redirect to Australia site
       window.location = 'https://acornsau.com.au/#get-notified-when-we-launch';
    } else if (!isRealEmail) {
     $('#email-input').addClass('subscribed-error');
    } else {
     $('#email-input').removeClass('subscribed-error');
      onsubmit(email, 'Web');
    }

    if (window.Acorns.countrySelectVisible) {
      if (!window.Acorns.countrySelected) {
        $('.country_error').slideDown();
      }
    }
  });

  function validateEmail(emailAddress) {
    var pattern = new RegExp(/^[+a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i);
    return pattern.test(emailAddress);
  }

  function lowercase(s) {
    return s ? s.toLowerCase() : s;
  }

  function classes($el) {
    return $el.attr('class');
  }

  function getPlatform($el) {
    return state.platforms.reduce(function(acc, v, i) {
      acc = !acc && ~classes($el).indexOf(lowercase(v)) ? v : acc;
      return acc;
    }, null);
  }

  function onsubmit(email, platform) {
    var data = ['email=' + encodeURIComponent(email)];
    if (platform) {
      data.push('&os=' + platform);
    }
    if (window.Acorns.countrySelected) {
      $.each(window.Acorns.countries, function(i, o) {
        if (o.name === window.Acorns.countrySelected) {
          data.push('&country=' + o.code);
        }
      });
    } else {
      return false;
    }
    $.ajax({
      url: 'https://api.acorns.com/v1/user/subscribe',
      type: 'POST',
      data: data.join(''),
    }).done(function(response) {
      $('.form-fail').removeClass('show');
      globalSection.removeClass('globalShowForm');
      $('.section.global .section-description').remove();
      globalSection.addClass('success');
    }).fail(function(response) {
      $('.form-fail').addClass('show');
    })
  }
  /*
   * Validate email on input blur
   */
  $('#email-input').blur(function() {
    var email = $(this).val(),
      isValid = validateEmail(email);

    if (isValid) {
      $('#email-input').removeClass('subscribed-error');
    } else {
     $('#email-input').addClass('subscribed-error');
    }
  });
});
(function($) {

  version = '0.0.2';

  $.fn.inputDropdown = function(options) {
    var settings, $input, $list, $listItems, $nextItem, $keyItem, $dropdownArrow, $dropdown, $dropdownInner, $body, list, dropdownHeight, dropdownOpened, selectedValue, currentBackgroundImageValue, keys;

    keys = {
      DOWN: 40,
      UP: 38,
      ENTER: 13
    };

    function log(args) {
      args = [].slice.call(arguments);
      var ret = args.reduce(function(acc, val, i) {
        return acc.concat(typeof val === 'object' ? JSON.stringify(val) : val);
      }, []);
      if (settings.debug && window.console) {
        console.log.apply(console, ret);
      }
    }

    function isTypeOf(v, type) {
      return typeof v === type;
    }

    function not(fn) {
      return !fn;
    }

    function fail(error) {
      throw error;
    }

    function result(o) {
      if (typeof settings.data === 'function') {
        return o();
      }
      return o;
    }

    function dashify(str) {
      return str.replace(/\s+/, '-');
    }

    function blur() {
      document.activeElement.blur();
    }

    function lowercase(v) {
      return v.toLowerCase();
    }

    function buildList(o, i) {
      var item = ['<li>','<span>',o,'</span>','</li>'];
      if (settings.image) {
        item.splice(1,0,'<img src="',settings.image(o),'" alt="">');
      }
      return item.join('');
    }

    function listItemSelect() {
      var $this = $(this),
      value = $this.text();
      if (settings.image) {
        var imageUrl = $this.find('img').attr('src');
        log('Selected image: ' + imageUrl);
        updateInputBg(imageUrl);
      }
      $input.val(value);
      closeDropdown();
      setSelectedValue(value);
      blur();
    }

    function selectFirst() {
      listItemSelect.call($listItems.filter(':visible').eq(0));
    }

    function select(value) {
      listItemSelect.call($listItems.filter(function() {
        return lowercase($(this).text()) === lowercase(value);
      }).eq(0));
    }

    function setSelectedValue(value) {
      selectedValue = value;
      log('Selected: ' + value);
      settings.onSelect(value);
    }

    function backgroundImageValue() {
      var url = $input.css('background-image');
      url = /^url\((['"]?)(.*)\1\)$/.exec(url);
      url = url ? url[2] : '';
      url = url.replace([window.location.protocol,'//',window.location.host].join(''), '');
        return url;
    }

    function updateInputBg(imageUrl) {
      var image = 'none';
      if (imageUrl) {
        image = 'url(' + imageUrl + ')';
      }
      currentBackgroundImageValue = imageUrl;
      $input.css({
        'background-image': image
      });
    }

    function openDropdown() {
      if (dropdownOpened) {
        closeDropdown();
      } else {
        $body.addClass('input-dropdown-opened');
        $dropdown.show();
        dropdownOpened = true;
        $dropdown.show();
        updateDropdownPosition();
      }
    }

    function closeDropdown() {
      $body.removeClass('input-dropdown-opened');
      $dropdown.hide();
      dropdownOpened = false;
    }

    function updateDropdownPosition() {
      var offsetTop = $input.offset().top + $input.outerHeight(),
      offsetLeft = $input.offset().left;
      log('offsetTop: ' + offsetTop);
      log('offsetLeft: ' + offsetLeft);
      dropdownHeight = $(window).height() - offsetTop - 20;
      $dropdown.css({
        'max-height': dropdownHeight,
        position: 'fixed',
        top: offsetTop,
        left: offsetLeft,
        width: $input.outerWidth()
      });
      $dropdownInner.css({
        'max-height': dropdownHeight,
        'overflow': 'auto',
        '-webkit-overflow-scrolling': 'touch'
      });
      log('Dropdown offsetTop: ' + offsetTop);
    }

    function inputKeyUp(e) {
      var $this = $(this),
      value = $this.val().toLowerCase();
      setSelectedValue(null);

      if (e.keyCode === 13) {
        e.preventDefault();
        selectFirst();
        return;
      }

      if (e.keyCode === 8 || e.keyCode === 46) {
        currentBackgroundImageValue = null;
      }

      if (!dropdownOpened) {
        $input.on('keyup', openDropdown);
      }

      if (settings.image && settings.defaultImage) {
        log('backgroundImage: ' + backgroundImageValue());
        log('currentBackgroundImage: ' + currentBackgroundImageValue);
        if (backgroundImageValue() !== currentBackgroundImageValue) {
          log('Image set');
          updateInputBg(settings.defaultImage);
        }
      }

      if (~$.map(settings.data, lowercase).indexOf(lowercase(value))) {
        log('Match: ' + value);
        select(value);
      }

      $listItems.each(function() {
        if (!!~$(this).text().toLowerCase().search(value)) {
          $(this).show();
        } else {
          $(this).hide();
        }
      });
    }

    function onEsc() {
      closeDropdown();
    }

    function docKeyUp(e) {
      if (e.keyCode === 27) {
        onEsc();
      }
    }

    function docKeyDown(e) {
      if (!dropdownOpened) return;

      if (e.which === keys.DOWN) {
        if (!dropdownOpened) {
          /*
           * Not sure why this was here in the first place.
           * -> openDropdown();
           */
        }
        if ($keyItem) {
          if ($keyItem.index('li') === $listItems.last().index('li')) { return false; }
          if ($keyItem.position().top + $keyItem.outerHeight()*2 >= $dropdownInner.outerHeight()) {
            $dropdownInner.scrollTop($dropdownInner.scrollTop() + $keyItem.outerHeight());
          }
          $keyItem.removeClass(settings.highlightClass);
          $nextItem = $keyItem.nextAll('li:visible').eq(0);
          if ($nextItem.length > 0){
            $keyItem = $nextItem.addClass(settings.highlightClass);
          } else {
            $keyItem = $listItems.filter(':visible').eq(0).addClass(settings.highlightClass);
          }
        } else {
          $keyItem = $listItems.filter(':visible').eq(0).addClass(settings.highlightClass);
        }
        return false;
      } else if (e.which === keys.UP) {
        if ($keyItem) {
          if ($keyItem.index('li') === 0) { return false; }
          if ($keyItem.position().top === 0) {
            $dropdownInner.scrollTop($dropdownInner.scrollTop() + ($keyItem.position().top - $keyItem.outerHeight()));
          }
          $keyItem.removeClass(settings.highlightClass);
          $nextItem = $keyItem.prevAll('li:visible').eq(0);
          if ($nextItem.length > 0) {
            $keyItem = $nextItem.addClass(settings.highlightClass);
          } else {
            $keyItem = $listItems.filter(':visible').last().addClass(settings.highlightClass);
          }
        } else {
          $keyItem = $listItems.filter(':visible').last().addClass(settings.highlightClass);
        }
        return false;
      }

      if (e.which === keys.ENTER) {
        if ($keyItem) {
          listItemSelect.call($keyItem);
          $keyItem = null;
        }
      }
    }

    function stripExceptLetters(c) {
      return c.replace(/[^a-zA-Z-_]/, '');
    }

    function init() {

      settings = $.extend({
        onSelect: null,
        data: null,
        toggleButton: null,
        image: false,
        debug: false,
        dropdownClass: '.dropdown',
        highlightClass: '.highlight'
      }, options);

      if (not(isTypeOf(settings.image, 'function'))) {
        fail(new TypeError('image must be a function'));
      }

      if (not(isTypeOf(settings.onSelect, 'function'))) {
        fail(new TypeError('onSelect must be a function'));
      }

      settings.data = result(settings.data);
      settings.highlightClass = stripExceptLetters(settings.highlightClass);

      if (!$.isArray(settings.data)) {
        fail(new TypeError('data must evaluate to an array'));
      }

      $body = $('body');
      $input = this;
      dropdownOpened = false;

      list = ['<div class="',stripExceptLetters(settings.dropdownClass),'"><div class="',stripExceptLetters(settings.dropdownClass) + '-inner','"><ul>',
        $.map(settings.data, buildList).join(''),
        '</ul></div></div>'].join('');

        $body.append(list);
        $dropdown = $(settings.dropdownClass);
        $dropdownInner = $(settings.dropdownClass + '-inner');
        $dropdown.hide();
        $list = $dropdown.find('ul');
        $listItems = $dropdown.find('li');
        currentBackgroundImageValue = settings.defaultImage;

        $input.on('keyup.input-dropdown', inputKeyUp);
        $listItems.on('click.input-dropdown', listItemSelect);
        $(document).on('keyup.input-dropdown', docKeyUp);
        $(window).on('resize.input-dropdown', updateDropdownPosition);
        $(window).on('keydown.input-dropdown', docKeyDown);
        if (settings.toggleButton) {
          $dropdownArrow = $(settings.toggleButton);
          $dropdownArrow.on('click.input-dropdown', openDropdown);
        }
    }

    init.apply(this, arguments);

    return {
      version: version
    };

  };

})(jQuery);

var handleRedirect = (function () {
    var redirectBrowser = function (site) {
      var uri = "https://" + site + ".acorns.com/";
      window.location = uri;
    };

    var sites = {
      "au": true
    };

    var defaultSite = "www";

    var onSuccess = function (geoipResponse) {
      if (!geoipResponse.country.iso_code) {
        redirectBrowser(defaultSite);
        return;
      }
      var code = geoipResponse.country.iso_code.toLowerCase();
      var wasRedirectedThisSession = sessionStorage.getItem('wasRedirectedThisSession');
      var notComingFromSisterSite = document.referrer.indexOf('acorns' + code +'.com.' + code + '/') === -1 ? true : false;

      if (sites[code] && wasRedirectedThisSession != 'true' && notComingFromSisterSite) {
        sessionStorage.setItem('wasRedirectedThisSession', true);
        redirectBrowser(code);
      }
    };

    var onError = function (error) {
      redirectBrowser(defaultSite);
    };

    return function () {
      // This may be blocked by AdBlock
      if (geoip2) geoip2.country( onSuccess, onError );
    };
}());

/*
 * Redirect once per session if user from Australia goes straight to acorns.com
 */
//handleRedirect();

if (isMobile()) {
  $('body').addClass('mobile');
}

// Adjust position of parallax image if landing page
if (window.location.pathname == '/lp1.html') {
  $('.lifestyle-overlay').css('top', '-300px');
  $('#logo').css('top', '20px');
}

var ticking = false;
/*
 * Detect mobile device
 */
detectDevice();

function detectDevice() {
  var uagent = navigator.userAgent.toLowerCase();
  if (uagent.search("iphone") > -1) $('body').addClass("iphone");
  else if (uagent.search("android") > -1) $('body').addClass("android");
}

function isiOS () {
  var uagent = navigator.userAgent.toLowerCase();
  if (uagent.search("iphone") > -1 || uagent.search("ipad") > -1) return true;
}

function isAndroid() {
  var uagent = navigator.userAgent.toLowerCase();
  if (uagent.search("android") > -1) return true;
}

/*
 * Scroll indicator
 */
$('.scroll-indicator').click(function() {
  $.smoothScroll({
    scrollTarget: '.features'
  });
  return false;
});

/* Change hrefs of 'Get Started Now' buttons
based on device */
function updateButtons() {
  var $getStartedButton;
  $getStartedButton = $('a.get-started-btn');

  if (window.location.pathname == '/lp1.html') {
    if (isiOS()) {
      $getStartedButton.attr('href', 'https://app.adjust.com/18kd1j');
    } else if (isAndroid()) {
      $getStartedButton.attr('href', 'https://app.adjust.com/bdev3y');
    }
  } else {
    if (isiOS()) {
      $getStartedButton.attr('href', 'https://app.adjust.io/luqqxi');
    } else if (isAndroid()) {
      $getStartedButton.attr('href', 'https://app.adjust.io/2bfkli');
    }
  }
}

window.onload = $(function(){
  updateButtons();

  // Map Animation
  setTimeout(function(){
    var section = $('.global');
    section.prepend(mapRight);
    section.prepend(mapLeft);

    if (document.querySelector('svg.map')) {
      var animatedMap = new AnimatedMap();
    }
  }, 0);
});

function AnimatedMap(options) {
  this.regions = [].slice.call(document.querySelectorAll('svg.map'));
  this.dots = [].slice.call(this.regions[0].getElementsByTagName('path'));
  this.dots = this.dots.concat([].slice.call(this.regions[1].getElementsByTagName('path')));
  var self = this;
  // Set initial state
  for (var i = 0; i < this.dots.length; ++i) {
    var dot = this.dots[i];
    dot.setAttribute('opacity', '0.25')
  }
  // Select a random dot and animate
  function flashDot() {
    // Choose random dot
    var num = Math.floor(Math.random() * self.dots.length - 1) + 1;
    var chosen = self.dots[num];
    chosen.setAttribute('class', 'fill');
    //Performance?
    setTimeout(function() {
      chosen.setAttribute('class', '');
    }, 3000);
  }
  var stop = false;
  var frameCount = 0;
  var fps, fpsInterval, startTime, now, then, elapsed;
  var mapContainer = document.getElementsByClassName('global')[0];
  if (!isMobile()) startAnimating(8, flashDot);
  // initialize the timer variables and start the animation
  function startAnimating(fps, render) {
    fpsInterval = 1000 / fps;
    then = Date.now();
    startTime = then;
    animate();
  }

  function animate() {
    // request another frame
    requestAnimationFrame(animate);
    // calc elapsed time since last loop
    now = Date.now();
    elapsed = now - then;
    // if enough time has elapsed, draw the next frame
    if (elapsed > fpsInterval && elementInViewport(mapContainer)) {
      // Get ready for next frame by setting then=now, but also adjust for your
      // specified fpsInterval not being a multiple of RAF's interval (16.7ms)
      then = now - (elapsed % fpsInterval);
      // Put your drawing code here
      flashDot();
    }
  }
}

function isMobile() {
  var check = false;
  (function(a) {
    if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4))) check = true
  })(navigator.userAgent || navigator.vendor || window.opera);
  return check;
}

function elementInViewport(el) {
   var top = el.offsetTop;
  var left = el.offsetLeft;
  var width = el.offsetWidth;
  var height = el.offsetHeight;

  while(el.offsetParent) {
    el = el.offsetParent;
    top += el.offsetTop;
    left += el.offsetLeft;
  }

  return (
    top < (window.pageYOffset + window.innerHeight) &&
    left < (window.pageXOffset + window.innerWidth) &&
    (top + height) > window.pageYOffset &&
    (left + width) > window.pageXOffset
  );
}

/*
 * Lifestyle, features text animations
 */
$('.features .container').appear();
$('.features .container').on('appear', function(event, $all_appeared_elements) {
  $all_appeared_elements.addClass('appeared');
});

$('.lifestyle .container').appear();
$('.lifestyle .container').on('appear', function(event, $all_appeared_elements) {
  $all_appeared_elements.addClass('appeared');
});
/*
 * Header fade on scroll
 */
$(function() {

  $window = $(window);
  $body = $("body");
  var hero = $('.hero');
  var heroText = $('.hero .container');
  var header = document.querySelector('header');
  var scrollFlag = false;
  window.addEventListener('scroll', function() {
    // Grab distance from top
    var scrollTop = $window.scrollTop();
    // Check/set scroll flag
    if (!scrollFlag) {
      scrollFlag = true;
      // Disable pointer events
      //$body.addClass("disable-pointer-events");
    }
    // Speed things up by debouncing pointer events
    //debouncePointerEvents();
    // Animate UI
    if (!isMobile()) fadeHero(scrollTop);
    if (scrollTop > 200) header.classList.add('header-shadow');
    else header.classList.remove('header-shadow');
  });

  function fadeHero(scrollTop) {
    var diff = (scrollTop / 670);
    // Smoothly translate text up to a point
    if (scrollTop >= 0 && scrollTop <= 670) {
      heroText.css({
        'transform': 'translate3d(0, ' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, ' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-moz-transform': 'translate3d(0, ' + Math.floor(scrollTop / 6) + 'px, 0)'
      });
      $('.macbook').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 2.5) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 2.5) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 2.5) + 'px, 0)'
      });
      $('.iphone-6').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)'
      });
      $('.iphone-6-plus').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)'
      });
      $('.s6').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 3) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 3) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 3) + 'px, 0)'
      });
      $('.apple-watch').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)'
      });
      $('.android-watch').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 4) + 'px, 0)'
      });
      $('.text').css({
        'transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-webkit-transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)',
        '-moz-transform': 'translate3d(0, -' + Math.floor(scrollTop / 6) + 'px, 0)'
      });
    }
    // Fade out text and fade in overlay
    if (diff < 1) {
      heroText.css({
        'opacity': 1 - diff
      });
    }
    // Fully hide hero so it doesnt cover parallax window
    if (scrollTop > 800) {
      hero.css('opacity', '0');
    } else {
      hero.css('opacity', '1');
    }
  }
  // Speed up things by disabling pointer events. I use TweenMax delayedCall instead JS native setInterval just for the sake of showing how to use this method. See more at http://www.thecssninja.com/javascript/pointer-events-60fps
  function debouncePointerEvents() {
    addPointerEvents();
  }

  function addPointerEvents() {
    scrollFlag = false;
    $body.removeClass("disable-pointer-events");
  }
});
/*
 * Global signup
 */
$(document).ready(function() {
  $('.global-signup').click(function() {
    $('.global').addClass('globalShowForm');
    setTimeout(function() {
      $('.global-signup span').fadeOut(300);
      $('.global-signup span').text('Notify Me');
      $('.global-signup span').fadeIn(300);
    }, 100);
  });
});
/*
 * Show disclaimers in portfolios section
 */
var portfoliosExpanded = false,
  parallaxes = document.getElementsByClassName("parallax-mirror"); // Two copies of parralax (for blur)
function shiftParallax(element, y, delay) {
  move(element).to(0, y).delay(delay).duration('0.3s').ease('linear').end(function() {
    $(element).toggleClass('parallax-fix')
  });
}
$('.disclaimers-btn').click(function() {
  $('.portfolios').toggleClass('disclaimers');
});
/*
 * Countries dropdown
 */
$(function() {
  if (!$('.indpn-input').length) return;
  $('.indpn-input').inputDropdown({
    data: function() {
      return $.map(window.Acorns.countries, function(o, i) {
        return o.name;
      });
    },
    onSelect: function(selected) {
      window.Acorns.countrySelected = selected;
      if (selected) {
        $('.country_error').hide();
        var $emailInput;
        $emailInput = $('#email-input');

        if (selected == 'Australia') {
          $emailInput.hide();
          $('#subscribe-email-button').text('Sign up for Acorns in Australia');
          $('#subscribe-email-button').addClass('australia');
        } else {
          $emailInput.show();
           $('#subscribe-email-button').text('Yes, Notify Me');
           $('#subscribe-email-button').removeClass('australia');
        }
      }
    },
    toggleButton: $('.indpn-dropdown-arrow'),
    dropdownClass: '.indpn-dropdown',
    image: function(item) {
      return ['/images/flags/32/', item.replace(/\s+/g, '_'), '.png'].join('');
    },
    defaultImage: '/images/country-select/search.png',
    debug: false
  });
  $(document).on('click.country', '.country-select', function(e) {
    $('.form-success').hide();
    $('.form-fail').hide();
    window.Acorns.countrySelectVisible = true;
    $('.country-select-info').hide();
    $('.toggle-button-container').hide();
    $('.indpn-input-container').show();
    $('.indpn-input').delay(100).focus();
  });

});

// Exclude lp1 from parallax

  /*
   * parallax
   */

  var transforms = ["transform", "msTransform", "webkitTransform", "mozTransform", "oTransform"];

  var scrolling = false;
  var mouseWheelActive = false;
  var count = 0;
  var mouseDelta = 0
  //var transformProperty;
  var imageContainer;
  var imageContainer = document.querySelector(".parallax-image");
  var transformProperty = getSupportedPropertyName(transforms);
  //
  // vendor prefix management
  //
  function getSupportedPropertyName(properties) {
    for (var i = 0; i < properties.length; i++) {
      if (typeof document.body.style[properties[i]] != "undefined") {
        return properties[i];
      }
    }
    return null;
  }

  function setup() {
    window.addEventListener("scroll", setScrolling, false);
    animationLoop();
  }
  if (!isMobile() && window.location.pathname !== '/lp1.html') setup();

  //
  // Called when a scroll is detected
  //
  function setScrolling() {
    scrolling = true;
  }
  //
  // Cross-browser way to get the current scroll position
  //
  function getScrollPosition() {
    if (document.documentElement.scrollTop == 0) {
      return document.body.scrollTop;
    } else {
      return document.documentElement.scrollTop;
    }
  }
  //
  // A performant way to shift our image up or down
  //
  function setTranslate3DTransform(element, yPosition) {
    var value = 'translateY(' + yPosition + 'px) translateZ(0px)';
    element.style[transformProperty] = value;
  }



function animationLoop() {
  // adjust the image's position when scrolling
  if (scrolling) {
    setTranslate3DTransform(imageContainer, -1 * getScrollPosition() / 5);
    scrolling = false;
  }
  // scroll up or down by 10 pixels when the mousewheel is used
  if (mouseWheelActive) {
    window.scrollBy(0, -mouseDelta * 10);
    count++;
    // stop the scrolling after a few moments
    if (count > 20) {
      count = 0;
      mouseWheelActive = false;
      mouseDelta = 0;
    }
  }
  requestAnimationFrame(animationLoop);
}
/*
 * Show get app modal on button click
 */
$('.get-app-btn').click(function() {
  var modal = document.getElementById('get-app-modal');

  if (isiOS()) window.open("https://app.adjust.io/luqqxi");
  else if (isAndroid())  window.open("https://app.adjust.io/2bfkli");
  else modal.show();
});

/*
 * Setup downtime mointor
 */
var DowntimeMonitorService = new DowntimeMonitor('https://s3.amazonaws.com/app.acorns.com/downtime.json');

// Banner
var banner = document.querySelector('.Downtime-Banner'),
  bannerCloseBtn = document.querySelector('.Downtime-Banner__close-button');

if (DowntimeMonitorService.status == 'down') {
  showDowntimeBanner();
}


// function showDowntimeBanner (downtime) {
//   // Move page down
//   var header = document.querySelector('#acorns-header');

//   setTimeout(function () {
//     var transform = 'translateY(74px)';
//     if(($(window).width() > 480) || (!isMobile())) header.style[transformProperty] = transform;
//   }, 1000);

//   // Set downtime text
//   document.querySelector('.Downtime-Banner__text p').innerHTML = downtime.description;
//   banner.classList.add('Downtime-Banner--show');
// }

// function hideDowntimeBanner () {
//   banner.classList.add('Downtime-Banner---slideOutUp');
//   setTimeout(function () {
//     banner.classList.remove('Downtime-Banner--show');
//   }, 500);

//   var header = document.querySelector('#acorns-header'),
//   transform = 'translateY(0px)';
//   header.style[transformProperty] = transform;
// }

// bannerCloseBtn.onclick = function () {
//   hideDowntimeBanner();
// }

// DowntimeMonitorService.on('start-downtime', function (downtime) {
//   showDowntimeBanner(downtime);
// });

// DowntimeMonitorService.on('end-downtime', function (downtime) {
//   hideDowntimeBanner();
// });


var mapLeft = '<svg class="map map-left" width="850px" height="1091px" viewBox="0 0 850 1091" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:sketch="http://www.bohemiancoding.com/sketch/ns"><defs><linearGradient id="Gradient" x1="0%" y1="0%" x2="0%" y2="100%" gradientUnits="userSpaceOnUse"><stop stop-color="#FFFFFF" offset="0%"></stop><stop stop-color="#B1FF87" offset="22.9970504%"></stop><stop stop-color="#30B096" offset="100%"></stop></linearGradient></defs><g id="Comp-3" stroke="none" stroke-width="1" fill="none" fill-rule="evenodd" sketch:type="MSPage"><g id="Artboard-13" sketch:type="MSArtboardGroup" transform="translate(-447.000000, -920.000000)" fill="url(#Gradient)"><g id="World-Map-Left" sketch:type="MSLayerGroup" transform="translate(447.000000, 920.000000)"><path d="M18.664,196.922 C18.664,201.856 14.665,205.855 9.731,205.855 C4.798,205.855 0.798,201.856 0.798,196.922 C0.798,191.989 4.798,187.989 9.731,187.989 C14.665,187.989 18.664,191.989 18.664,196.922" id="Fill-8" sketch:type="MSShapeGroup"></path><path d="M18.664,223.721 C18.664,228.654 14.665,232.654 9.731,232.654 C4.798,232.654 0.798,228.654 0.798,223.721 C0.798,218.787 4.798,214.788 9.731,214.788 C14.665,214.788 18.664,218.787 18.664,223.721" id="Fill-9" sketch:type="MSShapeGroup"></path><path d="M18.664,250.519 C18.664,255.453 14.665,259.452 9.731,259.452 C4.798,259.452 0.798,255.453 0.798,250.519 C0.798,245.586 4.798,241.586 9.731,241.586 C14.665,241.586 18.664,245.586 18.664,250.519" id="Fill-10" sketch:type="MSShapeGroup"></path><path d="M18.664,277.318 C18.664,282.251 14.665,286.251 9.731,286.251 C4.798,286.251 0.798,282.251 0.798,277.318 C0.798,272.384 4.798,268.385 9.731,268.385 C14.665,268.385 18.664,272.384 18.664,277.318" id="Fill-11" sketch:type="MSShapeGroup"></path><path d="M18.664,304.116 C18.664,309.05 14.665,313.049 9.731,313.049 C4.798,313.049 0.798,309.05 0.798,304.116 C0.798,299.183 4.798,295.184 9.731,295.184 C14.665,295.184 18.664,299.183 18.664,304.116" id="Fill-12" sketch:type="MSShapeGroup"></path><path d="M18.664,330.915 C18.664,335.848 14.665,339.848 9.731,339.848 C4.798,339.848 0.798,335.848 0.798,330.915 C0.798,325.981 4.798,321.982 9.731,321.982 C14.665,321.982 18.664,325.981 18.664,330.915" id="Fill-13" sketch:type="MSShapeGroup"></path><path d="M45.463,196.922 C45.463,201.856 41.463,205.855 36.53,205.855 C31.596,205.855 27.597,201.856 27.597,196.922 C27.597,191.989 31.596,187.989 36.53,187.989 C41.463,187.989 45.463,191.989 45.463,196.922" id="Fill-14" sketch:type="MSShapeGroup"></path><path d="M45.463,223.721 C45.463,228.654 41.463,232.654 36.53,232.654 C31.596,232.654 27.597,228.654 27.597,223.721 C27.597,218.787 31.596,214.788 36.53,214.788 C41.463,214.788 45.463,218.787 45.463,223.721" id="Fill-15" sketch:type="MSShapeGroup"></path><path d="M45.463,250.519 C45.463,255.453 41.463,259.452 36.53,259.452 C31.596,259.452 27.597,255.453 27.597,250.519 C27.597,245.586 31.596,241.586 36.53,241.586 C41.463,241.586 45.463,245.586 45.463,250.519" id="Fill-16" sketch:type="MSShapeGroup"></path><path d="M45.463,277.318 C45.463,282.251 41.463,286.251 36.53,286.251 C31.596,286.251 27.597,282.251 27.597,277.318 C27.597,272.384 31.596,268.385 36.53,268.385 C41.463,268.385 45.463,272.384 45.463,277.318" id="Fill-17" sketch:type="MSShapeGroup"></path><path d="M45.463,304.116 C45.463,309.05 41.463,313.049 36.53,313.049 C31.596,313.049 27.597,309.05 27.597,304.116 C27.597,299.183 31.596,295.184 36.53,295.184 C41.463,295.184 45.463,299.183 45.463,304.116" id="Fill-18" sketch:type="MSShapeGroup"></path><path d="M45.463,330.915 C45.463,335.848 41.463,339.848 36.53,339.848 C31.596,339.848 27.597,335.848 27.597,330.915 C27.597,325.981 31.596,321.982 36.53,321.982 C41.463,321.982 45.463,325.981 45.463,330.915" id="Fill-19" sketch:type="MSShapeGroup"></path><path d="M72.261,196.922 C72.261,201.856 68.262,205.855 63.328,205.855 C58.395,205.855 54.395,201.856 54.395,196.922 C54.395,191.989 58.395,187.989 63.328,187.989 C68.262,187.989 72.261,191.989 72.261,196.922" id="Fill-20" sketch:type="MSShapeGroup"></path><path d="M72.261,223.721 C72.261,228.654 68.262,232.654 63.328,232.654 C58.395,232.654 54.395,228.654 54.395,223.721 C54.395,218.787 58.395,214.788 63.328,214.788 C68.262,214.788 72.261,218.787 72.261,223.721" id="Fill-21" sketch:type="MSShapeGroup"></path><path d="M72.261,250.519 C72.261,255.453 68.262,259.452 63.328,259.452 C58.395,259.452 54.395,255.453 54.395,250.519 C54.395,245.586 58.395,241.586 63.328,241.586 C68.262,241.586 72.261,245.586 72.261,250.519" id="Fill-22" sketch:type="MSShapeGroup"></path><path d="M72.261,277.318 C72.261,282.251 68.262,286.251 63.328,286.251 C58.395,286.251 54.395,282.251 54.395,277.318 C54.395,272.384 58.395,268.385 63.328,268.385 C68.262,268.385 72.261,272.384 72.261,277.318" id="Fill-23" sketch:type="MSShapeGroup"></path><path d="M72.261,304.116 C72.261,309.05 68.262,313.049 63.328,313.049 C58.395,313.049 54.395,309.05 54.395,304.116 C54.395,299.183 58.395,295.184 63.328,295.184 C68.262,295.184 72.261,299.183 72.261,304.116" id="Fill-24" sketch:type="MSShapeGroup"></path><path d="M72.261,330.915 C72.261,335.848 68.262,339.848 63.328,339.848 C58.395,339.848 54.395,335.848 54.395,330.915 C54.395,325.981 58.395,321.982 63.328,321.982 C68.262,321.982 72.261,325.981 72.261,330.915" id="Fill-25" sketch:type="MSShapeGroup"></path><path d="M99.06,196.922 C99.06,201.856 95.06,205.855 90.127,205.855 C85.193,205.855 81.194,201.856 81.194,196.922 C81.194,191.989 85.193,187.989 90.127,187.989 C95.06,187.989 99.06,191.989 99.06,196.922" id="Fill-26" sketch:type="MSShapeGroup"></path><path d="M99.06,223.721 C99.06,228.654 95.06,232.654 90.127,232.654 C85.193,232.654 81.194,228.654 81.194,223.721 C81.194,218.787 85.193,214.788 90.127,214.788 C95.06,214.788 99.06,218.787 99.06,223.721" id="Fill-27" sketch:type="MSShapeGroup"></path><path d="M99.06,250.519 C99.06,255.453 95.06,259.452 90.127,259.452 C85.193,259.452 81.194,255.453 81.194,250.519 C81.194,245.586 85.193,241.586 90.127,241.586 C95.06,241.586 99.06,245.586 99.06,250.519" id="Fill-28" sketch:type="MSShapeGroup"></path><path d="M99.06,277.318 C99.06,282.251 95.06,286.251 90.127,286.251 C85.193,286.251 81.194,282.251 81.194,277.318 C81.194,272.384 85.193,268.385 90.127,268.385 C95.06,268.385 99.06,272.384 99.06,277.318" id="Fill-29" sketch:type="MSShapeGroup"></path><path d="M125.858,196.922 C125.858,201.856 121.859,205.855 116.925,205.855 C111.992,205.855 107.992,201.856 107.992,196.922 C107.992,191.989 111.992,187.989 116.925,187.989 C121.859,187.989 125.858,191.989 125.858,196.922" id="Fill-30" sketch:type="MSShapeGroup"></path><path d="M125.858,223.721 C125.858,228.654 121.859,232.654 116.925,232.654 C111.992,232.654 107.992,228.654 107.992,223.721 C107.992,218.787 111.992,214.788 116.925,214.788 C121.859,214.788 125.858,218.787 125.858,223.721" id="Fill-31" sketch:type="MSShapeGroup"></path><path d="M125.858,250.519 C125.858,255.453 121.859,259.452 116.925,259.452 C111.992,259.452 107.992,255.453 107.992,250.519 C107.992,245.586 111.992,241.586 116.925,241.586 C121.859,241.586 125.858,245.586 125.858,250.519" id="Fill-32" sketch:type="MSShapeGroup"></path><path d="M125.858,277.318 C125.858,282.251 121.859,286.251 116.925,286.251 C111.992,286.251 107.992,282.251 107.992,277.318 C107.992,272.384 111.992,268.385 116.925,268.385 C121.859,268.385 125.858,272.384 125.858,277.318" id="Fill-33" sketch:type="MSShapeGroup"></path><path d="M125.858,304.116 C125.858,309.05 121.859,313.049 116.925,313.049 C111.992,313.049 107.992,309.05 107.992,304.116 C107.992,299.183 111.992,295.184 116.925,295.184 C121.859,295.184 125.858,299.183 125.858,304.116" id="Fill-34" sketch:type="MSShapeGroup"></path><path d="M152.657,223.721 C152.657,228.654 148.657,232.654 143.724,232.654 C138.79,232.654 134.791,228.654 134.791,223.721 C134.791,218.787 138.79,214.788 143.724,214.788 C148.657,214.788 152.657,218.787 152.657,223.721" id="Fill-35" sketch:type="MSShapeGroup"></path><path d="M152.657,250.519 C152.657,255.453 148.657,259.452 143.724,259.452 C138.79,259.452 134.791,255.453 134.791,250.519 C134.791,245.586 138.79,241.586 143.724,241.586 C148.657,241.586 152.657,245.586 152.657,250.519" id="Fill-36" sketch:type="MSShapeGroup"></path><path d="M152.657,277.318 C152.657,282.251 148.657,286.251 143.724,286.251 C138.79,286.251 134.791,282.251 134.791,277.318 C134.791,272.384 138.79,268.385 143.724,268.385 C148.657,268.385 152.657,272.384 152.657,277.318" id="Fill-37" sketch:type="MSShapeGroup"></path><path d="M152.657,304.116 C152.657,309.05 148.657,313.049 143.724,313.049 C138.79,313.049 134.791,309.05 134.791,304.116 C134.791,299.183 138.79,295.184 143.724,295.184 C148.657,295.184 152.657,299.183 152.657,304.116" id="Fill-38" sketch:type="MSShapeGroup"></path><path d="M179.455,223.721 C179.455,228.654 175.456,232.654 170.522,232.654 C165.589,232.654 161.589,228.654 161.589,223.721 C161.589,218.787 165.589,214.788 170.522,214.788 C175.456,214.788 179.455,218.787 179.455,223.721" id="Fill-39" sketch:type="MSShapeGroup"></path><path d="M179.455,250.519 C179.455,255.453 175.456,259.452 170.522,259.452 C165.589,259.452 161.589,255.453 161.589,250.519 C161.589,245.586 165.589,241.586 170.522,241.586 C175.456,241.586 179.455,245.586 179.455,250.519" id="Fill-40" sketch:type="MSShapeGroup"></path><path d="M179.455,277.318 C179.455,282.251 175.456,286.251 170.522,286.251 C165.589,286.251 161.589,282.251 161.589,277.318 C161.589,272.384 165.589,268.385 170.522,268.385 C175.456,268.385 179.455,272.384 179.455,277.318" id="Fill-41" sketch:type="MSShapeGroup"></path><path d="M179.455,304.116 C179.455,309.05 175.456,313.049 170.522,313.049 C165.589,313.049 161.589,309.05 161.589,304.116 C161.589,299.183 165.589,295.184 170.522,295.184 C175.456,295.184 179.455,299.183 179.455,304.116" id="Fill-42" sketch:type="MSShapeGroup"></path><path d="M179.455,330.915 C179.455,335.848 175.456,339.848 170.522,339.848 C165.589,339.848 161.589,335.848 161.589,330.915 C161.589,325.981 165.589,321.982 170.522,321.982 C175.456,321.982 179.455,325.981 179.455,330.915" id="Fill-43" sketch:type="MSShapeGroup"></path><path d="M206.254,196.922 C206.254,201.856 202.254,205.855 197.321,205.855 C192.387,205.855 188.388,201.856 188.388,196.922 C188.388,191.989 192.387,187.989 197.321,187.989 C202.254,187.989 206.254,191.989 206.254,196.922" id="Fill-44" sketch:type="MSShapeGroup"></path><path d="M206.254,223.721 C206.254,228.654 202.254,232.654 197.321,232.654 C192.387,232.654 188.388,228.654 188.388,223.721 C188.388,218.787 192.387,214.788 197.321,214.788 C202.254,214.788 206.254,218.787 206.254,223.721" id="Fill-45" sketch:type="MSShapeGroup"></path><path d="M206.254,250.519 C206.254,255.453 202.254,259.452 197.321,259.452 C192.387,259.452 188.388,255.453 188.388,250.519 C188.388,245.586 192.387,241.586 197.321,241.586 C202.254,241.586 206.254,245.586 206.254,250.519" id="Fill-46" sketch:type="MSShapeGroup"></path><path d="M206.254,277.318 C206.254,282.251 202.254,286.251 197.321,286.251 C192.387,286.251 188.388,282.251 188.388,277.318 C188.388,272.384 192.387,268.385 197.321,268.385 C202.254,268.385 206.254,272.384 206.254,277.318" id="Fill-47" sketch:type="MSShapeGroup"></path><path d="M206.254,304.116 C206.254,309.05 202.254,313.049 197.321,313.049 C192.387,313.049 188.388,309.05 188.388,304.116 C188.388,299.183 192.387,295.184 197.321,295.184 C202.254,295.184 206.254,299.183 206.254,304.116" id="Fill-48" sketch:type="MSShapeGroup"></path><path d="M206.254,330.915 C206.254,335.848 202.254,339.848 197.321,339.848 C192.387,339.848 188.388,335.848 188.388,330.915 C188.388,325.981 192.387,321.982 197.321,321.982 C202.254,321.982 206.254,325.981 206.254,330.915" id="Fill-49" sketch:type="MSShapeGroup"></path><path d="M206.254,357.713 C206.254,362.647 202.254,366.646 197.321,366.646 C192.387,366.646 188.388,362.647 188.388,357.713 C188.388,352.78 192.387,348.78 197.321,348.78 C202.254,348.78 206.254,352.78 206.254,357.713" id="Fill-50" sketch:type="MSShapeGroup"></path><path d="M233.052,170.124 C233.052,175.057 229.053,179.057 224.119,179.057 C219.186,179.057 215.186,175.057 215.186,170.124 C215.186,165.19 219.186,161.191 224.119,161.191 C229.053,161.191 233.052,165.19 233.052,170.124" id="Fill-51" sketch:type="MSShapeGroup"></path><path d="M233.052,196.922 C233.052,201.856 229.053,205.855 224.119,205.855 C219.186,205.855 215.186,201.856 215.186,196.922 C215.186,191.989 219.186,187.989 224.119,187.989 C229.053,187.989 233.052,191.989 233.052,196.922" id="Fill-52" sketch:type="MSShapeGroup"></path><path d="M233.052,223.721 C233.052,228.654 229.053,232.654 224.119,232.654 C219.186,232.654 215.186,228.654 215.186,223.721 C215.186,218.787 219.186,214.788 224.119,214.788 C229.053,214.788 233.052,218.787 233.052,223.721" id="Fill-53" sketch:type="MSShapeGroup"></path><path d="M233.052,250.519 C233.052,255.453 229.053,259.452 224.119,259.452 C219.186,259.452 215.186,255.453 215.186,250.519 C215.186,245.586 219.186,241.586 224.119,241.586 C229.053,241.586 233.052,245.586 233.052,250.519" id="Fill-54" sketch:type="MSShapeGroup"></path><path d="M233.052,277.318 C233.052,282.251 229.053,286.251 224.119,286.251 C219.186,286.251 215.186,282.251 215.186,277.318 C215.186,272.384 219.186,268.385 224.119,268.385 C229.053,268.385 233.052,272.384 233.052,277.318" id="Fill-55" sketch:type="MSShapeGroup"></path><path d="M233.052,304.116 C233.052,309.05 229.053,313.049 224.119,313.049 C219.186,313.049 215.186,309.05 215.186,304.116 C215.186,299.183 219.186,295.184 224.119,295.184 C229.053,295.184 233.052,299.183 233.052,304.116" id="Fill-56" sketch:type="MSShapeGroup"></path><path d="M233.052,330.915 C233.052,335.848 229.053,339.848 224.119,339.848 C219.186,339.848 215.186,335.848 215.186,330.915 C215.186,325.981 219.186,321.982 224.119,321.982 C229.053,321.982 233.052,325.981 233.052,330.915" id="Fill-57" sketch:type="MSShapeGroup"></path><path d="M233.052,357.713 C233.052,362.647 229.053,366.646 224.119,366.646 C219.186,366.646 215.186,362.647 215.186,357.713 C215.186,352.78 219.186,348.78 224.119,348.78 C229.053,348.78 233.052,352.78 233.052,357.713" id="Fill-58" sketch:type="MSShapeGroup"></path><path d="M233.052,384.512 C233.052,389.445 229.053,393.445 224.119,393.445 C219.186,393.445 215.186,389.445 215.186,384.512 C215.186,379.578 219.186,375.579 224.119,375.579 C229.053,375.579 233.052,379.578 233.052,384.512" id="Fill-59" sketch:type="MSShapeGroup"></path><path d="M259.851,116.527 C259.851,121.46 255.851,125.46 250.918,125.46 C245.984,125.46 241.985,121.46 241.985,116.527 C241.985,111.593 245.984,107.594 250.918,107.594 C255.851,107.594 259.851,111.593 259.851,116.527" id="Fill-60" sketch:type="MSShapeGroup"></path><path d="M259.851,143.325 C259.851,148.259 255.851,152.258 250.918,152.258 C245.984,152.258 241.985,148.259 241.985,143.325 C241.985,138.392 245.984,134.392 250.918,134.392 C255.851,134.392 259.851,138.392 259.851,143.325" id="Fill-61" sketch:type="MSShapeGroup"></path><path d="M259.851,170.124 C259.851,175.057 255.851,179.057 250.918,179.057 C245.984,179.057 241.985,175.057 241.985,170.124 C241.985,165.19 245.984,161.191 250.918,161.191 C255.851,161.191 259.851,165.19 259.851,170.124" id="Fill-62" sketch:type="MSShapeGroup"></path><path d="M259.851,223.721 C259.851,228.654 255.851,232.654 250.918,232.654 C245.984,232.654 241.985,228.654 241.985,223.721 C241.985,218.787 245.984,214.788 250.918,214.788 C255.851,214.788 259.851,218.787 259.851,223.721" id="Fill-63" sketch:type="MSShapeGroup"></path><path d="M259.851,250.519 C259.851,255.453 255.851,259.452 250.918,259.452 C245.984,259.452 241.985,255.453 241.985,250.519 C241.985,245.586 245.984,241.586 250.918,241.586 C255.851,241.586 259.851,245.586 259.851,250.519" id="Fill-64" sketch:type="MSShapeGroup"></path><path d="M259.851,277.318 C259.851,282.251 255.851,286.251 250.918,286.251 C245.984,286.251 241.985,282.251 241.985,277.318 C241.985,272.384 245.984,268.385 250.918,268.385 C255.851,268.385 259.851,272.384 259.851,277.318" id="Fill-65" sketch:type="MSShapeGroup"></path><path d="M259.851,304.116 C259.851,309.05 255.851,313.049 250.918,313.049 C245.984,313.049 241.985,309.05 241.985,304.116 C241.985,299.183 245.984,295.184 250.918,295.184 C255.851,295.184 259.851,299.183 259.851,304.116" id="Fill-66" sketch:type="MSShapeGroup"></path><path d="M259.851,330.915 C259.851,335.848 255.851,339.848 250.918,339.848 C245.984,339.848 241.985,335.848 241.985,330.915 C241.985,325.981 245.984,321.982 250.918,321.982 C255.851,321.982 259.851,325.981 259.851,330.915" id="Fill-67" sketch:type="MSShapeGroup"></path><path d="M259.851,357.713 C259.851,362.647 255.851,366.646 250.918,366.646 C245.984,366.646 241.985,362.647 241.985,357.713 C241.985,352.78 245.984,348.78 250.918,348.78 C255.851,348.78 259.851,352.78 259.851,357.713" id="Fill-68" sketch:type="MSShapeGroup"></path><path d="M259.851,384.512 C259.851,389.445 255.851,393.445 250.918,393.445 C245.984,393.445 241.985,389.445 241.985,384.512 C241.985,379.578 245.984,375.579 250.918,375.579 C255.851,375.579 259.851,379.578 259.851,384.512" id="Fill-69" sketch:type="MSShapeGroup"></path><path d="M259.851,411.31 C259.851,416.244 255.851,420.243 250.918,420.243 C245.984,420.243 241.985,416.244 241.985,411.31 C241.985,406.377 245.984,402.378 250.918,402.378 C255.851,402.378 259.851,406.377 259.851,411.31" id="Fill-70" sketch:type="MSShapeGroup"></path><path d="M259.851,438.109 C259.851,443.042 255.851,447.042 250.918,447.042 C245.984,447.042 241.985,443.042 241.985,438.109 C241.985,433.175 245.984,429.176 250.918,429.176 C255.851,429.176 259.851,433.175 259.851,438.109" id="Fill-71" sketch:type="MSShapeGroup"></path><path d="M259.851,464.907 C259.851,469.841 255.851,473.84 250.918,473.84 C245.984,473.84 241.985,469.841 241.985,464.907 C241.985,459.974 245.984,455.975 250.918,455.975 C255.851,455.975 259.851,459.974 259.851,464.907" id="Fill-72" sketch:type="MSShapeGroup"></path><path d="M259.851,491.706 C259.851,496.639 255.851,500.639 250.918,500.639 C245.984,500.639 241.985,496.639 241.985,491.706 C241.985,486.772 245.984,482.773 250.918,482.773 C255.851,482.773 259.851,486.772 259.851,491.706" id="Fill-73" sketch:type="MSShapeGroup"></path><path d="M286.649,116.527 C286.649,121.46 282.65,125.46 277.716,125.46 C272.783,125.46 268.783,121.46 268.783,116.527 C268.783,111.593 272.783,107.594 277.716,107.594 C282.65,107.594 286.649,111.593 286.649,116.527" id="Fill-74" sketch:type="MSShapeGroup"></path><path d="M286.649,143.325 C286.649,148.259 282.65,152.258 277.716,152.258 C272.783,152.258 268.783,148.259 268.783,143.325 C268.783,138.392 272.783,134.392 277.716,134.392 C282.65,134.392 286.649,138.392 286.649,143.325" id="Fill-75" sketch:type="MSShapeGroup"></path><path d="M286.649,170.124 C286.649,175.057 282.65,179.057 277.716,179.057 C272.783,179.057 268.783,175.057 268.783,170.124 C268.783,165.19 272.783,161.191 277.716,161.191 C282.65,161.191 286.649,165.19 286.649,170.124" id="Fill-76" sketch:type="MSShapeGroup"></path><path d="M286.649,196.922 C286.649,201.856 282.65,205.855 277.716,205.855 C272.783,205.855 268.783,201.856 268.783,196.922 C268.783,191.989 272.783,187.989 277.716,187.989 C282.65,187.989 286.649,191.989 286.649,196.922" id="Fill-77" sketch:type="MSShapeGroup"></path><path d="M286.649,223.721 C286.649,228.654 282.65,232.654 277.716,232.654 C272.783,232.654 268.783,228.654 268.783,223.721 C268.783,218.787 272.783,214.788 277.716,214.788 C282.65,214.788 286.649,218.787 286.649,223.721" id="Fill-78" sketch:type="MSShapeGroup"></path><path d="M286.649,250.519 C286.649,255.453 282.65,259.452 277.716,259.452 C272.783,259.452 268.783,255.453 268.783,250.519 C268.783,245.586 272.783,241.586 277.716,241.586 C282.65,241.586 286.649,245.586 286.649,250.519" id="Fill-79" sketch:type="MSShapeGroup"></path><path d="M286.649,277.318 C286.649,282.251 282.65,286.251 277.716,286.251 C272.783,286.251 268.783,282.251 268.783,277.318 C268.783,272.384 272.783,268.385 277.716,268.385 C282.65,268.385 286.649,272.384 286.649,277.318" id="Fill-80" sketch:type="MSShapeGroup"></path><path d="M286.649,304.116 C286.649,309.05 282.65,313.049 277.716,313.049 C272.783,313.049 268.783,309.05 268.783,304.116 C268.783,299.183 272.783,295.184 277.716,295.184 C282.65,295.184 286.649,299.183 286.649,304.116" id="Fill-81" sketch:type="MSShapeGroup"></path><path d="M286.649,330.915 C286.649,335.848 282.65,339.848 277.716,339.848 C272.783,339.848 268.783,335.848 268.783,330.915 C268.783,325.981 272.783,321.982 277.716,321.982 C282.65,321.982 286.649,325.981 286.649,330.915" id="Fill-82" sketch:type="MSShapeGroup"></path><path d="M286.649,357.713 C286.649,362.647 282.65,366.646 277.716,366.646 C272.783,366.646 268.783,362.647 268.783,357.713 C268.783,352.78 272.783,348.78 277.716,348.78 C282.65,348.78 286.649,352.78 286.649,357.713" id="Fill-83" sketch:type="MSShapeGroup"></path><path d="M286.649,384.512 C286.649,389.445 282.65,393.445 277.716,393.445 C272.783,393.445 268.783,389.445 268.783,384.512 C268.783,379.578 272.783,375.579 277.716,375.579 C282.65,375.579 286.649,379.578 286.649,384.512" id="Fill-84" sketch:type="MSShapeGroup"></path><path d="M286.649,411.31 C286.649,416.244 282.65,420.243 277.716,420.243 C272.783,420.243 268.783,416.244 268.783,411.31 C268.783,406.377 272.783,402.378 277.716,402.378 C282.65,402.378 286.649,406.377 286.649,411.31" id="Fill-85" sketch:type="MSShapeGroup"></path><path d="M286.649,438.109 C286.649,443.042 282.65,447.042 277.716,447.042 C272.783,447.042 268.783,443.042 268.783,438.109 C268.783,433.175 272.783,429.176 277.716,429.176 C282.65,429.176 286.649,433.175 286.649,438.109" id="Fill-86" sketch:type="MSShapeGroup"></path><path d="M286.649,464.907 C286.649,469.841 282.65,473.84 277.716,473.84 C272.783,473.84 268.783,469.841 268.783,464.907 C268.783,459.974 272.783,455.975 277.716,455.975 C282.65,455.975 286.649,459.974 286.649,464.907" id="Fill-87" sketch:type="MSShapeGroup"></path><path d="M286.649,491.706 C286.649,496.639 282.65,500.639 277.716,500.639 C272.783,500.639 268.783,496.639 268.783,491.706 C268.783,486.772 272.783,482.773 277.716,482.773 C282.65,482.773 286.649,486.772 286.649,491.706" id="Fill-88" sketch:type="MSShapeGroup"></path><path d="M286.649,518.504 C286.649,523.438 282.65,527.437 277.716,527.437 C272.783,527.437 268.783,523.438 268.783,518.504 C268.783,513.571 272.783,509.572 277.716,509.572 C282.65,509.572 286.649,513.571 286.649,518.504" id="Fill-89" sketch:type="MSShapeGroup"></path><path d="M313.448,116.527 C313.448,121.46 309.448,125.46 304.515,125.46 C299.581,125.46 295.582,121.46 295.582,116.527 C295.582,111.593 299.581,107.594 304.515,107.594 C309.448,107.594 313.448,111.593 313.448,116.527" id="Fill-90" sketch:type="MSShapeGroup"></path><path d="M313.448,143.325 C313.448,148.259 309.448,152.258 304.515,152.258 C299.581,152.258 295.582,148.259 295.582,143.325 C295.582,138.392 299.581,134.392 304.515,134.392 C309.448,134.392 313.448,138.392 313.448,143.325" id="Fill-91" sketch:type="MSShapeGroup"></path><path d="M313.448,170.124 C313.448,175.057 309.448,179.057 304.515,179.057 C299.581,179.057 295.582,175.057 295.582,170.124 C295.582,165.19 299.581,161.191 304.515,161.191 C309.448,161.191 313.448,165.19 313.448,170.124" id="Fill-92" sketch:type="MSShapeGroup"></path><path d="M313.448,196.922 C313.448,201.856 309.448,205.855 304.515,205.855 C299.581,205.855 295.582,201.856 295.582,196.922 C295.582,191.989 299.581,187.989 304.515,187.989 C309.448,187.989 313.448,191.989 313.448,196.922" id="Fill-93" sketch:type="MSShapeGroup"></path><path d="M313.448,223.721 C313.448,228.654 309.448,232.654 304.515,232.654 C299.581,232.654 295.582,228.654 295.582,223.721 C295.582,218.787 299.581,214.788 304.515,214.788 C309.448,214.788 313.448,218.787 313.448,223.721" id="Fill-94" sketch:type="MSShapeGroup"></path><path d="M313.448,250.519 C313.448,255.453 309.448,259.452 304.515,259.452 C299.581,259.452 295.582,255.453 295.582,250.519 C295.582,245.586 299.581,241.586 304.515,241.586 C309.448,241.586 313.448,245.586 313.448,250.519" id="Fill-95" sketch:type="MSShapeGroup"></path><path d="M313.448,277.318 C313.448,282.251 309.448,286.251 304.515,286.251 C299.581,286.251 295.582,282.251 295.582,277.318 C295.582,272.384 299.581,268.385 304.515,268.385 C309.448,268.385 313.448,272.384 313.448,277.318" id="Fill-96" sketch:type="MSShapeGroup"></path><path d="M313.448,304.116 C313.448,309.05 309.448,313.049 304.515,313.049 C299.581,313.049 295.582,309.05 295.582,304.116 C295.582,299.183 299.581,295.184 304.515,295.184 C309.448,295.184 313.448,299.183 313.448,304.116" id="Fill-97" sketch:type="MSShapeGroup"></path><path d="M313.448,330.915 C313.448,335.848 309.448,339.848 304.515,339.848 C299.581,339.848 295.582,335.848 295.582,330.915 C295.582,325.981 299.581,321.982 304.515,321.982 C309.448,321.982 313.448,325.981 313.448,330.915" id="Fill-98" sketch:type="MSShapeGroup"></path><path d="M313.448,357.713 C313.448,362.647 309.448,366.646 304.515,366.646 C299.581,366.646 295.582,362.647 295.582,357.713 C295.582,352.78 299.581,348.78 304.515,348.78 C309.448,348.78 313.448,352.78 313.448,357.713" id="Fill-99" sketch:type="MSShapeGroup"></path><path d="M313.448,384.512 C313.448,389.445 309.448,393.445 304.515,393.445 C299.581,393.445 295.582,389.445 295.582,384.512 C295.582,379.578 299.581,375.579 304.515,375.579 C309.448,375.579 313.448,379.578 313.448,384.512" id="Fill-100" sketch:type="MSShapeGroup"></path><path d="M313.448,411.31 C313.448,416.244 309.448,420.243 304.515,420.243 C299.581,420.243 295.582,416.244 295.582,411.31 C295.582,406.377 299.581,402.378 304.515,402.378 C309.448,402.378 313.448,406.377 313.448,411.31" id="Fill-101" sketch:type="MSShapeGroup"></path><path d="M313.448,438.109 C313.448,443.042 309.448,447.042 304.515,447.042 C299.581,447.042 295.582,443.042 295.582,438.109 C295.582,433.175 299.581,429.176 304.515,429.176 C309.448,429.176 313.448,433.175 313.448,438.109" id="Fill-102" sketch:type="MSShapeGroup"></path><path d="M313.448,464.907 C313.448,469.841 309.448,473.84 304.515,473.84 C299.581,473.84 295.582,469.841 295.582,464.907 C295.582,459.974 299.581,455.975 304.515,455.975 C309.448,455.975 313.448,459.974 313.448,464.907" id="Fill-103" sketch:type="MSShapeGroup"></path><path d="M313.448,491.706 C313.448,496.639 309.448,500.639 304.515,500.639 C299.581,500.639 295.582,496.639 295.582,491.706 C295.582,486.772 299.581,482.773 304.515,482.773 C309.448,482.773 313.448,486.772 313.448,491.706" id="Fill-104" sketch:type="MSShapeGroup"></path><path d="M313.448,518.504 C313.448,523.438 309.448,527.437 304.515,527.437 C299.581,527.437 295.582,523.438 295.582,518.504 C295.582,513.571 299.581,509.572 304.515,509.572 C309.448,509.572 313.448,513.571 313.448,518.504" id="Fill-105" sketch:type="MSShapeGroup"></path><path d="M313.448,545.303 C313.448,550.236 309.448,554.236 304.515,554.236 C299.581,554.236 295.582,550.236 295.582,545.303 C295.582,540.369 299.581,536.37 304.515,536.37 C309.448,536.37 313.448,540.369 313.448,545.303" id="Fill-106" sketch:type="MSShapeGroup"></path><path d="M313.448,572.101 C313.448,577.035 309.448,581.034 304.515,581.034 C299.581,581.034 295.582,577.035 295.582,572.101 C295.582,567.168 299.581,563.168 304.515,563.168 C309.448,563.168 313.448,567.168 313.448,572.101" id="Fill-107" sketch:type="MSShapeGroup"></path><path d="M340.246,143.325 C340.246,148.259 336.247,152.258 331.313,152.258 C326.38,152.258 322.38,148.259 322.38,143.325 C322.38,138.392 326.38,134.392 331.313,134.392 C336.247,134.392 340.246,138.392 340.246,143.325" id="Fill-108" sketch:type="MSShapeGroup"></path><path d="M340.246,170.124 C340.246,175.057 336.247,179.057 331.313,179.057 C326.38,179.057 322.38,175.057 322.38,170.124 C322.38,165.19 326.38,161.191 331.313,161.191 C336.247,161.191 340.246,165.19 340.246,170.124" id="Fill-109" sketch:type="MSShapeGroup"></path><path d="M340.246,196.922 C340.246,201.856 336.247,205.855 331.313,205.855 C326.38,205.855 322.38,201.856 322.38,196.922 C322.38,191.989 326.38,187.989 331.313,187.989 C336.247,187.989 340.246,191.989 340.246,196.922" id="Fill-110" sketch:type="MSShapeGroup"></path><path d="M340.246,223.721 C340.246,228.654 336.247,232.654 331.313,232.654 C326.38,232.654 322.38,228.654 322.38,223.721 C322.38,218.787 326.38,214.788 331.313,214.788 C336.247,214.788 340.246,218.787 340.246,223.721" id="Fill-111" sketch:type="MSShapeGroup"></path><path d="M340.246,250.519 C340.246,255.453 336.247,259.452 331.313,259.452 C326.38,259.452 322.38,255.453 322.38,250.519 C322.38,245.586 326.38,241.586 331.313,241.586 C336.247,241.586 340.246,245.586 340.246,250.519" id="Fill-112" sketch:type="MSShapeGroup"></path><path d="M340.246,277.318 C340.246,282.251 336.247,286.251 331.313,286.251 C326.38,286.251 322.38,282.251 322.38,277.318 C322.38,272.384 326.38,268.385 331.313,268.385 C336.247,268.385 340.246,272.384 340.246,277.318" id="Fill-113" sketch:type="MSShapeGroup"></path><path d="M340.246,304.116 C340.246,309.05 336.247,313.049 331.313,313.049 C326.38,313.049 322.38,309.05 322.38,304.116 C322.38,299.183 326.38,295.184 331.313,295.184 C336.247,295.184 340.246,299.183 340.246,304.116" id="Fill-114" sketch:type="MSShapeGroup"></path><path d="M340.246,330.915 C340.246,335.848 336.247,339.848 331.313,339.848 C326.38,339.848 322.38,335.848 322.38,330.915 C322.38,325.981 326.38,321.982 331.313,321.982 C336.247,321.982 340.246,325.981 340.246,330.915" id="Fill-115" sketch:type="MSShapeGroup"></path><path d="M340.246,357.713 C340.246,362.647 336.247,366.646 331.313,366.646 C326.38,366.646 322.38,362.647 322.38,357.713 C322.38,352.78 326.38,348.78 331.313,348.78 C336.247,348.78 340.246,352.78 340.246,357.713" id="Fill-116" sketch:type="MSShapeGroup"></path><path d="M340.246,384.512 C340.246,389.445 336.247,393.445 331.313,393.445 C326.38,393.445 322.38,389.445 322.38,384.512 C322.38,379.578 326.38,375.579 331.313,375.579 C336.247,375.579 340.246,379.578 340.246,384.512" id="Fill-117" sketch:type="MSShapeGroup"></path><path d="M340.246,411.31 C340.246,416.244 336.247,420.243 331.313,420.243 C326.38,420.243 322.38,416.244 322.38,411.31 C322.38,406.377 326.38,402.378 331.313,402.378 C336.247,402.378 340.246,406.377 340.246,411.31" id="Fill-118" sketch:type="MSShapeGroup"></path><path d="M340.246,438.109 C340.246,443.042 336.247,447.042 331.313,447.042 C326.38,447.042 322.38,443.042 322.38,438.109 C322.38,433.175 326.38,429.176 331.313,429.176 C336.247,429.176 340.246,433.175 340.246,438.109" id="Fill-119" sketch:type="MSShapeGroup"></path><path d="M340.246,464.907 C340.246,469.841 336.247,473.84 331.313,473.84 C326.38,473.84 322.38,469.841 322.38,464.907 C322.38,459.974 326.38,455.975 331.313,455.975 C336.247,455.975 340.246,459.974 340.246,464.907" id="Fill-120" sketch:type="MSShapeGroup"></path><path d="M340.246,491.706 C340.246,496.639 336.247,500.639 331.313,500.639 C326.38,500.639 322.38,496.639 322.38,491.706 C322.38,486.772 326.38,482.773 331.313,482.773 C336.247,482.773 340.246,486.772 340.246,491.706" id="Fill-121" sketch:type="MSShapeGroup"></path><path d="M340.246,518.504 C340.246,523.438 336.247,527.437 331.313,527.437 C326.38,527.437 322.38,523.438 322.38,518.504 C322.38,513.571 326.38,509.572 331.313,509.572 C336.247,509.572 340.246,513.571 340.246,518.504" id="Fill-122" sketch:type="MSShapeGroup"></path><path d="M340.246,545.303 C340.246,550.236 336.247,554.236 331.313,554.236 C326.38,554.236 322.38,550.236 322.38,545.303 C322.38,540.369 326.38,536.37 331.313,536.37 C336.247,536.37 340.246,540.369 340.246,545.303" id="Fill-123" sketch:type="MSShapeGroup"></path><path d="M340.246,572.101 C340.246,577.035 336.247,581.034 331.313,581.034 C326.38,581.034 322.38,577.035 322.38,572.101 C322.38,567.168 326.38,563.168 331.313,563.168 C336.247,563.168 340.246,567.168 340.246,572.101" id="Fill-124" sketch:type="MSShapeGroup"></path><path d="M367.045,196.922 C367.045,201.856 363.045,205.855 358.112,205.855 C353.178,205.855 349.179,201.856 349.179,196.922 C349.179,191.989 353.178,187.989 358.112,187.989 C363.045,187.989 367.045,191.989 367.045,196.922" id="Fill-125" sketch:type="MSShapeGroup"></path><path d="M367.045,223.721 C367.045,228.654 363.045,232.654 358.112,232.654 C353.178,232.654 349.179,228.654 349.179,223.721 C349.179,218.787 353.178,214.788 358.112,214.788 C363.045,214.788 367.045,218.787 367.045,223.721" id="Fill-126" sketch:type="MSShapeGroup"></path><path d="M367.045,250.519 C367.045,255.453 363.045,259.452 358.112,259.452 C353.178,259.452 349.179,255.453 349.179,250.519 C349.179,245.586 353.178,241.586 358.112,241.586 C363.045,241.586 367.045,245.586 367.045,250.519" id="Fill-127" sketch:type="MSShapeGroup"></path><path d="M367.045,277.318 C367.045,282.251 363.045,286.251 358.112,286.251 C353.178,286.251 349.179,282.251 349.179,277.318 C349.179,272.384 353.178,268.385 358.112,268.385 C363.045,268.385 367.045,272.384 367.045,277.318" id="Fill-128" sketch:type="MSShapeGroup"></path><path d="M367.045,304.116 C367.045,309.05 363.045,313.049 358.112,313.049 C353.178,313.049 349.179,309.05 349.179,304.116 C349.179,299.183 353.178,295.184 358.112,295.184 C363.045,295.184 367.045,299.183 367.045,304.116" id="Fill-129" sketch:type="MSShapeGroup"></path><path d="M367.045,330.915 C367.045,335.848 363.045,339.848 358.112,339.848 C353.178,339.848 349.179,335.848 349.179,330.915 C349.179,325.981 353.178,321.982 358.112,321.982 C363.045,321.982 367.045,325.981 367.045,330.915" id="Fill-130" sketch:type="MSShapeGroup"></path><path d="M367.045,357.713 C367.045,362.647 363.045,366.646 358.112,366.646 C353.178,366.646 349.179,362.647 349.179,357.713 C349.179,352.78 353.178,348.78 358.112,348.78 C363.045,348.78 367.045,352.78 367.045,357.713" id="Fill-131" sketch:type="MSShapeGroup"></path><path d="M367.045,384.512 C367.045,389.445 363.045,393.445 358.112,393.445 C353.178,393.445 349.179,389.445 349.179,384.512 C349.179,379.578 353.178,375.579 358.112,375.579 C363.045,375.579 367.045,379.578 367.045,384.512" id="Fill-132" sketch:type="MSShapeGroup"></path><path d="M367.045,411.31 C367.045,416.244 363.045,420.243 358.112,420.243 C353.178,420.243 349.179,416.244 349.179,411.31 C349.179,406.377 353.178,402.378 358.112,402.378 C363.045,402.378 367.045,406.377 367.045,411.31" id="Fill-133" sketch:type="MSShapeGroup"></path><path d="M367.045,438.109 C367.045,443.042 363.045,447.042 358.112,447.042 C353.178,447.042 349.179,443.042 349.179,438.109 C349.179,433.175 353.178,429.176 358.112,429.176 C363.045,429.176 367.045,433.175 367.045,438.109" id="Fill-134" sketch:type="MSShapeGroup"></path><path d="M367.045,464.907 C367.045,469.841 363.045,473.84 358.112,473.84 C353.178,473.84 349.179,469.841 349.179,464.907 C349.179,459.974 353.178,455.975 358.112,455.975 C363.045,455.975 367.045,459.974 367.045,464.907" id="Fill-135" sketch:type="MSShapeGroup"></path><path d="M367.045,491.706 C367.045,496.639 363.045,500.639 358.112,500.639 C353.178,500.639 349.179,496.639 349.179,491.706 C349.179,486.772 353.178,482.773 358.112,482.773 C363.045,482.773 367.045,486.772 367.045,491.706" id="Fill-136" sketch:type="MSShapeGroup"></path><path d="M367.045,518.504 C367.045,523.438 363.045,527.437 358.112,527.437 C353.178,527.437 349.179,523.438 349.179,518.504 C349.179,513.571 353.178,509.572 358.112,509.572 C363.045,509.572 367.045,513.571 367.045,518.504" id="Fill-137" sketch:type="MSShapeGroup"></path><path d="M367.045,545.303 C367.045,550.236 363.045,554.236 358.112,554.236 C353.178,554.236 349.179,550.236 349.179,545.303 C349.179,540.369 353.178,536.37 358.112,536.37 C363.045,536.37 367.045,540.369 367.045,545.303" id="Fill-138" sketch:type="MSShapeGroup"></path><path d="M367.045,572.101 C367.045,577.035 363.045,581.034 358.112,581.034 C353.178,581.034 349.179,577.035 349.179,572.101 C349.179,567.168 353.178,563.168 358.112,563.168 C363.045,563.168 367.045,567.168 367.045,572.101" id="Fill-139" sketch:type="MSShapeGroup"></path><path d="M367.045,598.9 C367.045,603.833 363.045,607.833 358.112,607.833 C353.178,607.833 349.179,603.833 349.179,598.9 C349.179,593.966 353.178,589.967 358.112,589.967 C363.045,589.967 367.045,593.966 367.045,598.9" id="Fill-140" sketch:type="MSShapeGroup"></path><path d="M393.843,143.325 C393.843,148.259 389.844,152.258 384.91,152.258 C379.977,152.258 375.977,148.259 375.977,143.325 C375.977,138.392 379.977,134.392 384.91,134.392 C389.844,134.392 393.843,138.392 393.843,143.325" id="Fill-141" sketch:type="MSShapeGroup"></path><path d="M393.843,170.124 C393.843,175.057 389.844,179.057 384.91,179.057 C379.977,179.057 375.977,175.057 375.977,170.124 C375.977,165.19 379.977,161.191 384.91,161.191 C389.844,161.191 393.843,165.19 393.843,170.124" id="Fill-142" sketch:type="MSShapeGroup"></path><path d="M393.843,196.922 C393.843,201.856 389.844,205.855 384.91,205.855 C379.977,205.855 375.977,201.856 375.977,196.922 C375.977,191.989 379.977,187.989 384.91,187.989 C389.844,187.989 393.843,191.989 393.843,196.922" id="Fill-143" sketch:type="MSShapeGroup"></path><path d="M393.843,223.721 C393.843,228.654 389.844,232.654 384.91,232.654 C379.977,232.654 375.977,228.654 375.977,223.721 C375.977,218.787 379.977,214.788 384.91,214.788 C389.844,214.788 393.843,218.787 393.843,223.721" id="Fill-144" sketch:type="MSShapeGroup"></path><path d="M393.843,250.519 C393.843,255.453 389.844,259.452 384.91,259.452 C379.977,259.452 375.977,255.453 375.977,250.519 C375.977,245.586 379.977,241.586 384.91,241.586 C389.844,241.586 393.843,245.586 393.843,250.519" id="Fill-145" sketch:type="MSShapeGroup"></path><path d="M393.843,277.318 C393.843,282.251 389.844,286.251 384.91,286.251 C379.977,286.251 375.977,282.251 375.977,277.318 C375.977,272.384 379.977,268.385 384.91,268.385 C389.844,268.385 393.843,272.384 393.843,277.318" id="Fill-146" sketch:type="MSShapeGroup"></path><path d="M393.843,304.116 C393.843,309.05 389.844,313.049 384.91,313.049 C379.977,313.049 375.977,309.05 375.977,304.116 C375.977,299.183 379.977,295.184 384.91,295.184 C389.844,295.184 393.843,299.183 393.843,304.116" id="Fill-147" sketch:type="MSShapeGroup"></path><path d="M393.843,330.915 C393.843,335.848 389.844,339.848 384.91,339.848 C379.977,339.848 375.977,335.848 375.977,330.915 C375.977,325.981 379.977,321.982 384.91,321.982 C389.844,321.982 393.843,325.981 393.843,330.915" id="Fill-148" sketch:type="MSShapeGroup"></path><path d="M393.843,357.713 C393.843,362.647 389.844,366.646 384.91,366.646 C379.977,366.646 375.977,362.647 375.977,357.713 C375.977,352.78 379.977,348.78 384.91,348.78 C389.844,348.78 393.843,352.78 393.843,357.713" id="Fill-149" sketch:type="MSShapeGroup"></path><path d="M393.843,384.512 C393.843,389.445 389.844,393.445 384.91,393.445 C379.977,393.445 375.977,389.445 375.977,384.512 C375.977,379.578 379.977,375.579 384.91,375.579 C389.844,375.579 393.843,379.578 393.843,384.512" id="Fill-150" sketch:type="MSShapeGroup"></path><path d="M393.843,411.31 C393.843,416.244 389.844,420.243 384.91,420.243 C379.977,420.243 375.977,416.244 375.977,411.31 C375.977,406.377 379.977,402.378 384.91,402.378 C389.844,402.378 393.843,406.377 393.843,411.31" id="Fill-151" sketch:type="MSShapeGroup"></path><path d="M393.843,438.109 C393.843,443.042 389.844,447.042 384.91,447.042 C379.977,447.042 375.977,443.042 375.977,438.109 C375.977,433.175 379.977,429.176 384.91,429.176 C389.844,429.176 393.843,433.175 393.843,438.109" id="Fill-152" sketch:type="MSShapeGroup"></path><path d="M393.843,464.907 C393.843,469.841 389.844,473.84 384.91,473.84 C379.977,473.84 375.977,469.841 375.977,464.907 C375.977,459.974 379.977,455.975 384.91,455.975 C389.844,455.975 393.843,459.974 393.843,464.907" id="Fill-153" sketch:type="MSShapeGroup"></path><path d="M393.843,491.706 C393.843,496.639 389.844,500.639 384.91,500.639 C379.977,500.639 375.977,496.639 375.977,491.706 C375.977,486.772 379.977,482.773 384.91,482.773 C389.844,482.773 393.843,486.772 393.843,491.706" id="Fill-154" sketch:type="MSShapeGroup"></path><path d="M393.843,518.504 C393.843,523.438 389.844,527.437 384.91,527.437 C379.977,527.437 375.977,523.438 375.977,518.504 C375.977,513.571 379.977,509.572 384.91,509.572 C389.844,509.572 393.843,513.571 393.843,518.504" id="Fill-155" sketch:type="MSShapeGroup"></path><path d="M393.843,545.303 C393.843,550.236 389.844,554.236 384.91,554.236 C379.977,554.236 375.977,550.236 375.977,545.303 C375.977,540.369 379.977,536.37 384.91,536.37 C389.844,536.37 393.843,540.369 393.843,545.303" id="Fill-156" sketch:type="MSShapeGroup"></path><path d="M393.843,572.101 C393.843,577.035 389.844,581.034 384.91,581.034 C379.977,581.034 375.977,577.035 375.977,572.101 C375.977,567.168 379.977,563.168 384.91,563.168 C389.844,563.168 393.843,567.168 393.843,572.101" id="Fill-157" sketch:type="MSShapeGroup"></path><path d="M393.843,598.9 C393.843,603.833 389.844,607.833 384.91,607.833 C379.977,607.833 375.977,603.833 375.977,598.9 C375.977,593.966 379.977,589.967 384.91,589.967 C389.844,589.967 393.843,593.966 393.843,598.9" id="Fill-158" sketch:type="MSShapeGroup"></path><path d="M420.642,62.93 C420.642,67.863 416.642,71.863 411.709,71.863 C406.775,71.863 402.776,67.863 402.776,62.93 C402.776,57.996 406.775,53.997 411.709,53.997 C416.642,53.997 420.642,57.996 420.642,62.93" id="Fill-159" sketch:type="MSShapeGroup"></path><path d="M420.642,89.729 C420.642,94.662 416.642,98.661 411.709,98.661 C406.775,98.661 402.776,94.662 402.776,89.729 C402.776,84.795 406.775,80.796 411.709,80.796 C416.642,80.796 420.642,84.795 420.642,89.729" id="Fill-160" sketch:type="MSShapeGroup"></path><path d="M420.642,143.325 C420.642,148.259 416.642,152.258 411.709,152.258 C406.775,152.258 402.776,148.259 402.776,143.325 C402.776,138.392 406.775,134.392 411.709,134.392 C416.642,134.392 420.642,138.392 420.642,143.325" id="Fill-161" sketch:type="MSShapeGroup"></path><path d="M420.642,170.124 C420.642,175.057 416.642,179.057 411.709,179.057 C406.775,179.057 402.776,175.057 402.776,170.124 C402.776,165.19 406.775,161.191 411.709,161.191 C416.642,161.191 420.642,165.19 420.642,170.124" id="Fill-162" sketch:type="MSShapeGroup"></path><path d="M420.642,196.922 C420.642,201.856 416.642,205.855 411.709,205.855 C406.775,205.855 402.776,201.856 402.776,196.922 C402.776,191.989 406.775,187.989 411.709,187.989 C416.642,187.989 420.642,191.989 420.642,196.922" id="Fill-163" sketch:type="MSShapeGroup"></path><path d="M420.642,223.721 C420.642,228.654 416.642,232.654 411.709,232.654 C406.775,232.654 402.776,228.654 402.776,223.721 C402.776,218.787 406.775,214.788 411.709,214.788 C416.642,214.788 420.642,218.787 420.642,223.721" id="Fill-164" sketch:type="MSShapeGroup"></path><path d="M420.642,250.519 C420.642,255.453 416.642,259.452 411.709,259.452 C406.775,259.452 402.776,255.453 402.776,250.519 C402.776,245.586 406.775,241.586 411.709,241.586 C416.642,241.586 420.642,245.586 420.642,250.519" id="Fill-165" sketch:type="MSShapeGroup"></path><path d="M420.642,277.318 C420.642,282.251 416.642,286.251 411.709,286.251 C406.775,286.251 402.776,282.251 402.776,277.318 C402.776,272.384 406.775,268.385 411.709,268.385 C416.642,268.385 420.642,272.384 420.642,277.318" id="Fill-166" sketch:type="MSShapeGroup"></path><path d="M420.642,304.116 C420.642,309.05 416.642,313.049 411.709,313.049 C406.775,313.049 402.776,309.05 402.776,304.116 C402.776,299.183 406.775,295.184 411.709,295.184 C416.642,295.184 420.642,299.183 420.642,304.116" id="Fill-167" sketch:type="MSShapeGroup"></path><path d="M420.642,330.915 C420.642,335.848 416.642,339.848 411.709,339.848 C406.775,339.848 402.776,335.848 402.776,330.915 C402.776,325.981 406.775,321.982 411.709,321.982 C416.642,321.982 420.642,325.981 420.642,330.915" id="Fill-168" sketch:type="MSShapeGroup"></path><path d="M420.642,357.713 C420.642,362.647 416.642,366.646 411.709,366.646 C406.775,366.646 402.776,362.647 402.776,357.713 C402.776,352.78 406.775,348.78 411.709,348.78 C416.642,348.78 420.642,352.78 420.642,357.713" id="Fill-169" sketch:type="MSShapeGroup"></path><path d="M420.642,384.512 C420.642,389.445 416.642,393.445 411.709,393.445 C406.775,393.445 402.776,389.445 402.776,384.512 C402.776,379.578 406.775,375.579 411.709,375.579 C416.642,375.579 420.642,379.578 420.642,384.512" id="Fill-170" sketch:type="MSShapeGroup"></path><path d="M420.642,411.31 C420.642,416.244 416.642,420.243 411.709,420.243 C406.775,420.243 402.776,416.244 402.776,411.31 C402.776,406.377 406.775,402.378 411.709,402.378 C416.642,402.378 420.642,406.377 420.642,411.31" id="Fill-171" sketch:type="MSShapeGroup"></path><path d="M420.642,438.109 C420.642,443.042 416.642,447.042 411.709,447.042 C406.775,447.042 402.776,443.042 402.776,438.109 C402.776,433.175 406.775,429.176 411.709,429.176 C416.642,429.176 420.642,433.175 420.642,438.109" id="Fill-172" sketch:type="MSShapeGroup"></path><path d="M420.642,464.907 C420.642,469.841 416.642,473.84 411.709,473.84 C406.775,473.84 402.776,469.841 402.776,464.907 C402.776,459.974 406.775,455.975 411.709,455.975 C416.642,455.975 420.642,459.974 420.642,464.907" id="Fill-173" sketch:type="MSShapeGroup"></path><path d="M420.642,491.706 C420.642,496.639 416.642,500.639 411.709,500.639 C406.775,500.639 402.776,496.639 402.776,491.706 C402.776,486.772 406.775,482.773 411.709,482.773 C416.642,482.773 420.642,486.772 420.642,491.706" id="Fill-174" sketch:type="MSShapeGroup"></path><path d="M420.642,518.504 C420.642,523.438 416.642,527.437 411.709,527.437 C406.775,527.437 402.776,523.438 402.776,518.504 C402.776,513.571 406.775,509.572 411.709,509.572 C416.642,509.572 420.642,513.571 420.642,518.504" id="Fill-175" sketch:type="MSShapeGroup"></path><path d="M420.642,598.9 C420.642,603.833 416.642,607.833 411.709,607.833 C406.775,607.833 402.776,603.833 402.776,598.9 C402.776,593.966 406.775,589.967 411.709,589.967 C416.642,589.967 420.642,593.966 420.642,598.9" id="Fill-176" sketch:type="MSShapeGroup"></path><path d="M420.642,625.698 C420.642,630.632 416.642,634.631 411.709,634.631 C406.775,634.631 402.776,630.632 402.776,625.698 C402.776,620.765 406.775,616.766 411.709,616.766 C416.642,616.766 420.642,620.765 420.642,625.698" id="Fill-177" sketch:type="MSShapeGroup"></path><path d="M447.44,62.93 C447.44,67.863 443.441,71.863 438.507,71.863 C433.574,71.863 429.574,67.863 429.574,62.93 C429.574,57.996 433.574,53.997 438.507,53.997 C443.441,53.997 447.44,57.996 447.44,62.93" id="Fill-178" sketch:type="MSShapeGroup"></path><path d="M447.44,89.729 C447.44,94.662 443.441,98.661 438.507,98.661 C433.574,98.661 429.574,94.662 429.574,89.729 C429.574,84.795 433.574,80.796 438.507,80.796 C443.441,80.796 447.44,84.795 447.44,89.729" id="Fill-179" sketch:type="MSShapeGroup"></path><path d="M447.44,116.527 C447.44,121.46 443.441,125.46 438.507,125.46 C433.574,125.46 429.574,121.46 429.574,116.527 C429.574,111.593 433.574,107.594 438.507,107.594 C443.441,107.594 447.44,111.593 447.44,116.527" id="Fill-180" sketch:type="MSShapeGroup"></path><path d="M447.44,143.325 C447.44,148.259 443.441,152.258 438.507,152.258 C433.574,152.258 429.574,148.259 429.574,143.325 C429.574,138.392 433.574,134.392 438.507,134.392 C443.441,134.392 447.44,138.392 447.44,143.325" id="Fill-181" sketch:type="MSShapeGroup"></path><path d="M447.44,170.124 C447.44,175.057 443.441,179.057 438.507,179.057 C433.574,179.057 429.574,175.057 429.574,170.124 C429.574,165.19 433.574,161.191 438.507,161.191 C443.441,161.191 447.44,165.19 447.44,170.124" id="Fill-182" sketch:type="MSShapeGroup"></path><path d="M447.44,196.922 C447.44,201.856 443.441,205.855 438.507,205.855 C433.574,205.855 429.574,201.856 429.574,196.922 C429.574,191.989 433.574,187.989 438.507,187.989 C443.441,187.989 447.44,191.989 447.44,196.922" id="Fill-183" sketch:type="MSShapeGroup"></path><path d="M447.44,223.721 C447.44,228.654 443.441,232.654 438.507,232.654 C433.574,232.654 429.574,228.654 429.574,223.721 C429.574,218.787 433.574,214.788 438.507,214.788 C443.441,214.788 447.44,218.787 447.44,223.721" id="Fill-184" sketch:type="MSShapeGroup"></path><path d="M447.44,250.519 C447.44,255.453 443.441,259.452 438.507,259.452 C433.574,259.452 429.574,255.453 429.574,250.519 C429.574,245.586 433.574,241.586 438.507,241.586 C443.441,241.586 447.44,245.586 447.44,250.519" id="Fill-185" sketch:type="MSShapeGroup"></path><path d="M447.44,357.713 C447.44,362.647 443.441,366.646 438.507,366.646 C433.574,366.646 429.574,362.647 429.574,357.713 C429.574,352.78 433.574,348.78 438.507,348.78 C443.441,348.78 447.44,352.78 447.44,357.713" id="Fill-186" sketch:type="MSShapeGroup"></path><path d="M447.44,384.512 C447.44,389.445 443.441,393.445 438.507,393.445 C433.574,393.445 429.574,389.445 429.574,384.512 C429.574,379.578 433.574,375.579 438.507,375.579 C443.441,375.579 447.44,379.578 447.44,384.512" id="Fill-187" sketch:type="MSShapeGroup"></path><path d="M447.44,411.31 C447.44,416.244 443.441,420.243 438.507,420.243 C433.574,420.243 429.574,416.244 429.574,411.31 C429.574,406.377 433.574,402.378 438.507,402.378 C443.441,402.378 447.44,406.377 447.44,411.31" id="Fill-188" sketch:type="MSShapeGroup"></path><path d="M447.44,438.109 C447.44,443.042 443.441,447.042 438.507,447.042 C433.574,447.042 429.574,443.042 429.574,438.109 C429.574,433.175 433.574,429.176 438.507,429.176 C443.441,429.176 447.44,433.175 447.44,438.109" id="Fill-189" sketch:type="MSShapeGroup"></path><path d="M447.44,464.907 C447.44,469.841 443.441,473.84 438.507,473.84 C433.574,473.84 429.574,469.841 429.574,464.907 C429.574,459.974 433.574,455.975 438.507,455.975 C443.441,455.975 447.44,459.974 447.44,464.907" id="Fill-190" sketch:type="MSShapeGroup"></path><path d="M447.44,491.706 C447.44,496.639 443.441,500.639 438.507,500.639 C433.574,500.639 429.574,496.639 429.574,491.706 C429.574,486.772 433.574,482.773 438.507,482.773 C443.441,482.773 447.44,486.772 447.44,491.706" id="Fill-191" sketch:type="MSShapeGroup"></path><path d="M447.44,518.504 C447.44,523.438 443.441,527.437 438.507,527.437 C433.574,527.437 429.574,523.438 429.574,518.504 C429.574,513.571 433.574,509.572 438.507,509.572 C443.441,509.572 447.44,513.571 447.44,518.504" id="Fill-192" sketch:type="MSShapeGroup"></path><path d="M447.44,598.9 C447.44,603.833 443.441,607.833 438.507,607.833 C433.574,607.833 429.574,603.833 429.574,598.9 C429.574,593.966 433.574,589.967 438.507,589.967 C443.441,589.967 447.44,593.966 447.44,598.9" id="Fill-193" sketch:type="MSShapeGroup"></path><path d="M447.44,625.698 C447.44,630.632 443.441,634.631 438.507,634.631 C433.574,634.631 429.574,630.632 429.574,625.698 C429.574,620.765 433.574,616.766 438.507,616.766 C443.441,616.766 447.44,620.765 447.44,625.698" id="Fill-194" sketch:type="MSShapeGroup"></path><path d="M474.239,62.93 C474.239,67.863 470.239,71.863 465.306,71.863 C460.372,71.863 456.373,67.863 456.373,62.93 C456.373,57.996 460.372,53.997 465.306,53.997 C470.239,53.997 474.239,57.996 474.239,62.93" id="Fill-195" sketch:type="MSShapeGroup"></path><path d="M474.239,89.729 C474.239,94.662 470.239,98.661 465.306,98.661 C460.372,98.661 456.373,94.662 456.373,89.729 C456.373,84.795 460.372,80.796 465.306,80.796 C470.239,80.796 474.239,84.795 474.239,89.729" id="Fill-196" sketch:type="MSShapeGroup"></path><path d="M474.239,116.527 C474.239,121.46 470.239,125.46 465.306,125.46 C460.372,125.46 456.373,121.46 456.373,116.527 C456.373,111.593 460.372,107.594 465.306,107.594 C470.239,107.594 474.239,111.593 474.239,116.527" id="Fill-197" sketch:type="MSShapeGroup"></path><path d="M474.239,143.325 C474.239,148.259 470.239,152.258 465.306,152.258 C460.372,152.258 456.373,148.259 456.373,143.325 C456.373,138.392 460.372,134.392 465.306,134.392 C470.239,134.392 474.239,138.392 474.239,143.325" id="Fill-198" sketch:type="MSShapeGroup"></path><path d="M474.239,170.124 C474.239,175.057 470.239,179.057 465.306,179.057 C460.372,179.057 456.373,175.057 456.373,170.124 C456.373,165.19 460.372,161.191 465.306,161.191 C470.239,161.191 474.239,165.19 474.239,170.124" id="Fill-199" sketch:type="MSShapeGroup"></path><path d="M474.239,196.922 C474.239,201.856 470.239,205.855 465.306,205.855 C460.372,205.855 456.373,201.856 456.373,196.922 C456.373,191.989 460.372,187.989 465.306,187.989 C470.239,187.989 474.239,191.989 474.239,196.922" id="Fill-200" sketch:type="MSShapeGroup"></path><path d="M474.239,223.721 C474.239,228.654 470.239,232.654 465.306,232.654 C460.372,232.654 456.373,228.654 456.373,223.721 C456.373,218.787 460.372,214.788 465.306,214.788 C470.239,214.788 474.239,218.787 474.239,223.721" id="Fill-201" sketch:type="MSShapeGroup"></path><path d="M474.239,250.519 C474.239,255.453 470.239,259.452 465.306,259.452 C460.372,259.452 456.373,255.453 456.373,250.519 C456.373,245.586 460.372,241.586 465.306,241.586 C470.239,241.586 474.239,245.586 474.239,250.519" id="Fill-202" sketch:type="MSShapeGroup"></path><path d="M474.239,384.512 C474.239,389.445 470.239,393.445 465.306,393.445 C460.372,393.445 456.373,389.445 456.373,384.512 C456.373,379.578 460.372,375.579 465.306,375.579 C470.239,375.579 474.239,379.578 474.239,384.512" id="Fill-203" sketch:type="MSShapeGroup"></path><path d="M474.239,411.31 C474.239,416.244 470.239,420.243 465.306,420.243 C460.372,420.243 456.373,416.244 456.373,411.31 C456.373,406.377 460.372,402.378 465.306,402.378 C470.239,402.378 474.239,406.377 474.239,411.31" id="Fill-204" sketch:type="MSShapeGroup"></path><path d="M474.239,438.109 C474.239,443.042 470.239,447.042 465.306,447.042 C460.372,447.042 456.373,443.042 456.373,438.109 C456.373,433.175 460.372,429.176 465.306,429.176 C470.239,429.176 474.239,433.175 474.239,438.109" id="Fill-205" sketch:type="MSShapeGroup"></path><path d="M474.239,464.907 C474.239,469.841 470.239,473.84 465.306,473.84 C460.372,473.84 456.373,469.841 456.373,464.907 C456.373,459.974 460.372,455.975 465.306,455.975 C470.239,455.975 474.239,459.974 474.239,464.907" id="Fill-206" sketch:type="MSShapeGroup"></path><path d="M474.239,491.706 C474.239,496.639 470.239,500.639 465.306,500.639 C460.372,500.639 456.373,496.639 456.373,491.706 C456.373,486.772 460.372,482.773 465.306,482.773 C470.239,482.773 474.239,486.772 474.239,491.706" id="Fill-207" sketch:type="MSShapeGroup"></path><path d="M474.239,518.504 C474.239,523.438 470.239,527.437 465.306,527.437 C460.372,527.437 456.373,523.438 456.373,518.504 C456.373,513.571 460.372,509.572 465.306,509.572 C470.239,509.572 474.239,513.571 474.239,518.504" id="Fill-208" sketch:type="MSShapeGroup"></path><path d="M474.239,598.9 C474.239,603.833 470.239,607.833 465.306,607.833 C460.372,607.833 456.373,603.833 456.373,598.9 C456.373,593.966 460.372,589.967 465.306,589.967 C470.239,589.967 474.239,593.966 474.239,598.9" id="Fill-209" sketch:type="MSShapeGroup"></path><path d="M474.239,625.698 C474.239,630.632 470.239,634.631 465.306,634.631 C460.372,634.631 456.373,630.632 456.373,625.698 C456.373,620.765 460.372,616.766 465.306,616.766 C470.239,616.766 474.239,620.765 474.239,625.698" id="Fill-210" sketch:type="MSShapeGroup"></path><path d="M474.239,652.497 C474.239,657.43 470.239,661.43 465.306,661.43 C460.372,661.43 456.373,657.43 456.373,652.497 C456.373,647.563 460.372,643.564 465.306,643.564 C470.239,643.564 474.239,647.563 474.239,652.497" id="Fill-211" sketch:type="MSShapeGroup"></path><path d="M474.239,679.295 C474.239,684.229 470.239,688.228 465.306,688.228 C460.372,688.228 456.373,684.229 456.373,679.295 C456.373,674.362 460.372,670.363 465.306,670.363 C470.239,670.363 474.239,674.362 474.239,679.295" id="Fill-212" sketch:type="MSShapeGroup"></path><path d="M474.239,706.094 C474.239,711.027 470.239,715.027 465.306,715.027 C460.372,715.027 456.373,711.027 456.373,706.094 C456.373,701.16 460.372,697.161 465.306,697.161 C470.239,697.161 474.239,701.16 474.239,706.094" id="Fill-213" sketch:type="MSShapeGroup"></path><path d="M474.239,732.892 C474.239,737.826 470.239,741.825 465.306,741.825 C460.372,741.825 456.373,737.826 456.373,732.892 C456.373,727.959 460.372,723.96 465.306,723.96 C470.239,723.96 474.239,727.959 474.239,732.892" id="Fill-214" sketch:type="MSShapeGroup"></path><path d="M474.239,759.691 C474.239,764.624 470.239,768.624 465.306,768.624 C460.372,768.624 456.373,764.624 456.373,759.691 C456.373,754.757 460.372,750.758 465.306,750.758 C470.239,750.758 474.239,754.757 474.239,759.691" id="Fill-215" sketch:type="MSShapeGroup"></path><path d="M474.239,786.489 C474.239,791.423 470.239,795.422 465.306,795.422 C460.372,795.422 456.373,791.423 456.373,786.489 C456.373,781.556 460.372,777.556 465.306,777.556 C470.239,777.556 474.239,781.556 474.239,786.489" id="Fill-216" sketch:type="MSShapeGroup"></path><path d="M501.037,62.93 C501.037,67.863 497.038,71.863 492.104,71.863 C487.171,71.863 483.171,67.863 483.171,62.93 C483.171,57.996 487.171,53.997 492.104,53.997 C497.038,53.997 501.037,57.996 501.037,62.93" id="Fill-217" sketch:type="MSShapeGroup"></path><path d="M501.037,89.729 C501.037,94.662 497.038,98.661 492.104,98.661 C487.171,98.661 483.171,94.662 483.171,89.729 C483.171,84.795 487.171,80.796 492.104,80.796 C497.038,80.796 501.037,84.795 501.037,89.729" id="Fill-218" sketch:type="MSShapeGroup"></path><path d="M501.037,116.527 C501.037,121.46 497.038,125.46 492.104,125.46 C487.171,125.46 483.171,121.46 483.171,116.527 C483.171,111.593 487.171,107.594 492.104,107.594 C497.038,107.594 501.037,111.593 501.037,116.527" id="Fill-219" sketch:type="MSShapeGroup"></path><path d="M501.037,143.325 C501.037,148.259 497.038,152.258 492.104,152.258 C487.171,152.258 483.171,148.259 483.171,143.325 C483.171,138.392 487.171,134.392 492.104,134.392 C497.038,134.392 501.037,138.392 501.037,143.325" id="Fill-220" sketch:type="MSShapeGroup"></path><path d="M501.037,170.124 C501.037,175.057 497.038,179.057 492.104,179.057 C487.171,179.057 483.171,175.057 483.171,170.124 C483.171,165.19 487.171,161.191 492.104,161.191 C497.038,161.191 501.037,165.19 501.037,170.124" id="Fill-221" sketch:type="MSShapeGroup"></path><path d="M501.037,196.922 C501.037,201.856 497.038,205.855 492.104,205.855 C487.171,205.855 483.171,201.856 483.171,196.922 C483.171,191.989 487.171,187.989 492.104,187.989 C497.038,187.989 501.037,191.989 501.037,196.922" id="Fill-222" sketch:type="MSShapeGroup"></path><path d="M501.037,223.721 C501.037,228.654 497.038,232.654 492.104,232.654 C487.171,232.654 483.171,228.654 483.171,223.721 C483.171,218.787 487.171,214.788 492.104,214.788 C497.038,214.788 501.037,218.787 501.037,223.721" id="Fill-223" sketch:type="MSShapeGroup"></path><path d="M501.037,250.519 C501.037,255.453 497.038,259.452 492.104,259.452 C487.171,259.452 483.171,255.453 483.171,250.519 C483.171,245.586 487.171,241.586 492.104,241.586 C497.038,241.586 501.037,245.586 501.037,250.519" id="Fill-224" sketch:type="MSShapeGroup"></path><path d="M501.037,384.512 C501.037,389.445 497.038,393.445 492.104,393.445 C487.171,393.445 483.171,389.445 483.171,384.512 C483.171,379.578 487.171,375.579 492.104,375.579 C497.038,375.579 501.037,379.578 501.037,384.512" id="Fill-225" sketch:type="MSShapeGroup"></path><path d="M501.037,411.31 C501.037,416.244 497.038,420.243 492.104,420.243 C487.171,420.243 483.171,416.244 483.171,411.31 C483.171,406.377 487.171,402.378 492.104,402.378 C497.038,402.378 501.037,406.377 501.037,411.31" id="Fill-226" sketch:type="MSShapeGroup"></path><path d="M501.037,438.109 C501.037,443.042 497.038,447.042 492.104,447.042 C487.171,447.042 483.171,443.042 483.171,438.109 C483.171,433.175 487.171,429.176 492.104,429.176 C497.038,429.176 501.037,433.175 501.037,438.109" id="Fill-227" sketch:type="MSShapeGroup"></path><path d="M501.037,464.907 C501.037,469.841 497.038,473.84 492.104,473.84 C487.171,473.84 483.171,469.841 483.171,464.907 C483.171,459.974 487.171,455.975 492.104,455.975 C497.038,455.975 501.037,459.974 501.037,464.907" id="Fill-228" sketch:type="MSShapeGroup"></path><path d="M501.037,491.706 C501.037,496.639 497.038,500.639 492.104,500.639 C487.171,500.639 483.171,496.639 483.171,491.706 C483.171,486.772 487.171,482.773 492.104,482.773 C497.038,482.773 501.037,486.772 501.037,491.706" id="Fill-229" sketch:type="MSShapeGroup"></path><path d="M501.037,518.504 C501.037,523.438 497.038,527.437 492.104,527.437 C487.171,527.437 483.171,523.438 483.171,518.504 C483.171,513.571 487.171,509.572 492.104,509.572 C497.038,509.572 501.037,513.571 501.037,518.504" id="Fill-230" sketch:type="MSShapeGroup"></path><path d="M501.037,545.303 C501.037,550.236 497.038,554.236 492.104,554.236 C487.171,554.236 483.171,550.236 483.171,545.303 C483.171,540.369 487.171,536.37 492.104,536.37 C497.038,536.37 501.037,540.369 501.037,545.303" id="Fill-231" sketch:type="MSShapeGroup"></path><path d="M501.037,572.101 C501.037,577.035 497.038,581.034 492.104,581.034 C487.171,581.034 483.171,577.035 483.171,572.101 C483.171,567.168 487.171,563.168 492.104,563.168 C497.038,563.168 501.037,567.168 501.037,572.101" id="Fill-232" sketch:type="MSShapeGroup"></path><path d="M501.037,625.698 C501.037,630.632 497.038,634.631 492.104,634.631 C487.171,634.631 483.171,630.632 483.171,625.698 C483.171,620.765 487.171,616.766 492.104,616.766 C497.038,616.766 501.037,620.765 501.037,625.698" id="Fill-233" sketch:type="MSShapeGroup"></path><path d="M501.037,652.497 C501.037,657.43 497.038,661.43 492.104,661.43 C487.171,661.43 483.171,657.43 483.171,652.497 C483.171,647.563 487.171,643.564 492.104,643.564 C497.038,643.564 501.037,647.563 501.037,652.497" id="Fill-234" sketch:type="MSShapeGroup"></path><path d="M501.037,679.295 C501.037,684.229 497.038,688.228 492.104,688.228 C487.171,688.228 483.171,684.229 483.171,679.295 C483.171,674.362 487.171,670.363 492.104,670.363 C497.038,670.363 501.037,674.362 501.037,679.295" id="Fill-235" sketch:type="MSShapeGroup"></path><path d="M501.037,706.094 C501.037,711.027 497.038,715.027 492.104,715.027 C487.171,715.027 483.171,711.027 483.171,706.094 C483.171,701.16 487.171,697.161 492.104,697.161 C497.038,697.161 501.037,701.16 501.037,706.094" id="Fill-236" sketch:type="MSShapeGroup"></path><path d="M501.037,732.892 C501.037,737.826 497.038,741.825 492.104,741.825 C487.171,741.825 483.171,737.826 483.171,732.892 C483.171,727.959 487.171,723.96 492.104,723.96 C497.038,723.96 501.037,727.959 501.037,732.892" id="Fill-237" sketch:type="MSShapeGroup"></path><path d="M501.037,759.691 C501.037,764.624 497.038,768.624 492.104,768.624 C487.171,768.624 483.171,764.624 483.171,759.691 C483.171,754.757 487.171,750.758 492.104,750.758 C497.038,750.758 501.037,754.757 501.037,759.691" id="Fill-238" sketch:type="MSShapeGroup"></path><path d="M501.037,786.489 C501.037,791.423 497.038,795.422 492.104,795.422 C487.171,795.422 483.171,791.423 483.171,786.489 C483.171,781.556 487.171,777.556 492.104,777.556 C497.038,777.556 501.037,781.556 501.037,786.489" id="Fill-239" sketch:type="MSShapeGroup"></path><path d="M501.037,813.288 C501.037,818.221 497.038,822.221 492.104,822.221 C487.171,822.221 483.171,818.221 483.171,813.288 C483.171,808.354 487.171,804.355 492.104,804.355 C497.038,804.355 501.037,808.354 501.037,813.288" id="Fill-240" sketch:type="MSShapeGroup"></path><path d="M501.037,840.086 C501.037,845.02 497.038,849.019 492.104,849.019 C487.171,849.019 483.171,845.02 483.171,840.086 C483.171,835.153 487.171,831.154 492.104,831.154 C497.038,831.154 501.037,835.153 501.037,840.086" id="Fill-241" sketch:type="MSShapeGroup"></path><path d="M501.037,866.885 C501.037,871.818 497.038,875.818 492.104,875.818 C487.171,875.818 483.171,871.818 483.171,866.885 C483.171,861.951 487.171,857.952 492.104,857.952 C497.038,857.952 501.037,861.951 501.037,866.885" id="Fill-242" sketch:type="MSShapeGroup"></path><path d="M501.037,893.683 C501.037,898.617 497.038,902.616 492.104,902.616 C487.171,902.616 483.171,898.617 483.171,893.683 C483.171,888.75 487.171,884.751 492.104,884.751 C497.038,884.751 501.037,888.75 501.037,893.683" id="Fill-243" sketch:type="MSShapeGroup"></path><path d="M527.836,62.93 C527.836,67.863 523.836,71.863 518.903,71.863 C513.969,71.863 509.97,67.863 509.97,62.93 C509.97,57.996 513.969,53.997 518.903,53.997 C523.836,53.997 527.836,57.996 527.836,62.93" id="Fill-244" sketch:type="MSShapeGroup"></path><path d="M527.836,89.729 C527.836,94.662 523.836,98.661 518.903,98.661 C513.969,98.661 509.97,94.662 509.97,89.729 C509.97,84.795 513.969,80.796 518.903,80.796 C523.836,80.796 527.836,84.795 527.836,89.729" id="Fill-245" sketch:type="MSShapeGroup"></path><path d="M527.836,116.527 C527.836,121.46 523.836,125.46 518.903,125.46 C513.969,125.46 509.97,121.46 509.97,116.527 C509.97,111.593 513.969,107.594 518.903,107.594 C523.836,107.594 527.836,111.593 527.836,116.527" id="Fill-246" sketch:type="MSShapeGroup"></path><path d="M527.836,143.325 C527.836,148.259 523.836,152.258 518.903,152.258 C513.969,152.258 509.97,148.259 509.97,143.325 C509.97,138.392 513.969,134.392 518.903,134.392 C523.836,134.392 527.836,138.392 527.836,143.325" id="Fill-247" sketch:type="MSShapeGroup"></path><path d="M527.836,170.124 C527.836,175.057 523.836,179.057 518.903,179.057 C513.969,179.057 509.97,175.057 509.97,170.124 C509.97,165.19 513.969,161.191 518.903,161.191 C523.836,161.191 527.836,165.19 527.836,170.124" id="Fill-248" sketch:type="MSShapeGroup"></path><path d="M527.836,196.922 C527.836,201.856 523.836,205.855 518.903,205.855 C513.969,205.855 509.97,201.856 509.97,196.922 C509.97,191.989 513.969,187.989 518.903,187.989 C523.836,187.989 527.836,191.989 527.836,196.922" id="Fill-249" sketch:type="MSShapeGroup"></path><path d="M527.836,223.721 C527.836,228.654 523.836,232.654 518.903,232.654 C513.969,232.654 509.97,228.654 509.97,223.721 C509.97,218.787 513.969,214.788 518.903,214.788 C523.836,214.788 527.836,218.787 527.836,223.721" id="Fill-250" sketch:type="MSShapeGroup"></path><path d="M527.836,250.519 C527.836,255.453 523.836,259.452 518.903,259.452 C513.969,259.452 509.97,255.453 509.97,250.519 C509.97,245.586 513.969,241.586 518.903,241.586 C523.836,241.586 527.836,245.586 527.836,250.519" id="Fill-251" sketch:type="MSShapeGroup"></path><path d="M527.836,277.318 C527.836,282.251 523.836,286.251 518.903,286.251 C513.969,286.251 509.97,282.251 509.97,277.318 C509.97,272.384 513.969,268.385 518.903,268.385 C523.836,268.385 527.836,272.384 527.836,277.318" id="Fill-252" sketch:type="MSShapeGroup"></path><path d="M527.836,304.116 C527.836,309.05 523.836,313.049 518.903,313.049 C513.969,313.049 509.97,309.05 509.97,304.116 C509.97,299.183 513.969,295.184 518.903,295.184 C523.836,295.184 527.836,299.183 527.836,304.116" id="Fill-253" sketch:type="MSShapeGroup"></path><path d="M527.836,330.915 C527.836,335.848 523.836,339.848 518.903,339.848 C513.969,339.848 509.97,335.848 509.97,330.915 C509.97,325.981 513.969,321.982 518.903,321.982 C523.836,321.982 527.836,325.981 527.836,330.915" id="Fill-254" sketch:type="MSShapeGroup"></path><path d="M527.836,357.713 C527.836,362.647 523.836,366.646 518.903,366.646 C513.969,366.646 509.97,362.647 509.97,357.713 C509.97,352.78 513.969,348.78 518.903,348.78 C523.836,348.78 527.836,352.78 527.836,357.713" id="Fill-255" sketch:type="MSShapeGroup"></path><path d="M527.836,384.512 C527.836,389.445 523.836,393.445 518.903,393.445 C513.969,393.445 509.97,389.445 509.97,384.512 C509.97,379.578 513.969,375.579 518.903,375.579 C523.836,375.579 527.836,379.578 527.836,384.512" id="Fill-256" sketch:type="MSShapeGroup"></path><path d="M527.836,411.31 C527.836,416.244 523.836,420.243 518.903,420.243 C513.969,420.243 509.97,416.244 509.97,411.31 C509.97,406.377 513.969,402.378 518.903,402.378 C523.836,402.378 527.836,406.377 527.836,411.31" id="Fill-257" sketch:type="MSShapeGroup"></path><path d="M527.836,438.109 C527.836,443.042 523.836,447.042 518.903,447.042 C513.969,447.042 509.97,443.042 509.97,438.109 C509.97,433.175 513.969,429.176 518.903,429.176 C523.836,429.176 527.836,433.175 527.836,438.109" id="Fill-258" sketch:type="MSShapeGroup"></path><path d="M527.836,464.907 C527.836,469.841 523.836,473.84 518.903,473.84 C513.969,473.84 509.97,469.841 509.97,464.907 C509.97,459.974 513.969,455.975 518.903,455.975 C523.836,455.975 527.836,459.974 527.836,464.907" id="Fill-259" sketch:type="MSShapeGroup"></path><path d="M527.836,491.706 C527.836,496.639 523.836,500.639 518.903,500.639 C513.969,500.639 509.97,496.639 509.97,491.706 C509.97,486.772 513.969,482.773 518.903,482.773 C523.836,482.773 527.836,486.772 527.836,491.706" id="Fill-260" sketch:type="MSShapeGroup"></path><path d="M527.836,598.9 C527.836,603.833 523.836,607.833 518.903,607.833 C513.969,607.833 509.97,603.833 509.97,598.9 C509.97,593.966 513.969,589.967 518.903,589.967 C523.836,589.967 527.836,593.966 527.836,598.9" id="Fill-261" sketch:type="MSShapeGroup"></path><path d="M527.836,679.295 C527.836,684.229 523.836,688.228 518.903,688.228 C513.969,688.228 509.97,684.229 509.97,679.295 C509.97,674.362 513.969,670.363 518.903,670.363 C523.836,670.363 527.836,674.362 527.836,679.295" id="Fill-262" sketch:type="MSShapeGroup"></path><path d="M527.836,706.094 C527.836,711.027 523.836,715.027 518.903,715.027 C513.969,715.027 509.97,711.027 509.97,706.094 C509.97,701.16 513.969,697.161 518.903,697.161 C523.836,697.161 527.836,701.16 527.836,706.094" id="Fill-263" sketch:type="MSShapeGroup"></path><path d="M527.836,732.892 C527.836,737.826 523.836,741.825 518.903,741.825 C513.969,741.825 509.97,737.826 509.97,732.892 C509.97,727.959 513.969,723.96 518.903,723.96 C523.836,723.96 527.836,727.959 527.836,732.892" id="Fill-264" sketch:type="MSShapeGroup"></path><path d="M527.836,759.691 C527.836,764.624 523.836,768.624 518.903,768.624 C513.969,768.624 509.97,764.624 509.97,759.691 C509.97,754.757 513.969,750.758 518.903,750.758 C523.836,750.758 527.836,754.757 527.836,759.691" id="Fill-265" sketch:type="MSShapeGroup"></path><path d="M527.836,786.489 C527.836,791.423 523.836,795.422 518.903,795.422 C513.969,795.422 509.97,791.423 509.97,786.489 C509.97,781.556 513.969,777.556 518.903,777.556 C523.836,777.556 527.836,781.556 527.836,786.489" id="Fill-266" sketch:type="MSShapeGroup"></path><path d="M527.836,813.288 C527.836,818.221 523.836,822.221 518.903,822.221 C513.969,822.221 509.97,818.221 509.97,813.288 C509.97,808.354 513.969,804.355 518.903,804.355 C523.836,804.355 527.836,808.354 527.836,813.288" id="Fill-267" sketch:type="MSShapeGroup"></path><path d="M527.836,840.086 C527.836,845.02 523.836,849.019 518.903,849.019 C513.969,849.019 509.97,845.02 509.97,840.086 C509.97,835.153 513.969,831.154 518.903,831.154 C523.836,831.154 527.836,835.153 527.836,840.086" id="Fill-268" sketch:type="MSShapeGroup"></path><path d="M527.836,866.885 C527.836,871.818 523.836,875.818 518.903,875.818 C513.969,875.818 509.97,871.818 509.97,866.885 C509.97,861.951 513.969,857.952 518.903,857.952 C523.836,857.952 527.836,861.951 527.836,866.885" id="Fill-269" sketch:type="MSShapeGroup"></path><path d="M527.836,893.683 C527.836,898.617 523.836,902.616 518.903,902.616 C513.969,902.616 509.97,898.617 509.97,893.683 C509.97,888.75 513.969,884.751 518.903,884.751 C523.836,884.751 527.836,888.75 527.836,893.683" id="Fill-270" sketch:type="MSShapeGroup"></path><path d="M527.836,920.482 C527.836,925.415 523.836,929.415 518.903,929.415 C513.969,929.415 509.97,925.415 509.97,920.482 C509.97,915.548 513.969,911.549 518.903,911.549 C523.836,911.549 527.836,915.548 527.836,920.482" id="Fill-271" sketch:type="MSShapeGroup"></path><path d="M527.836,947.28 C527.836,952.214 523.836,956.213 518.903,956.213 C513.969,956.213 509.97,952.214 509.97,947.28 C509.97,942.347 513.969,938.348 518.903,938.348 C523.836,938.348 527.836,942.347 527.836,947.28" id="Fill-272" sketch:type="MSShapeGroup"></path><path d="M527.836,974.079 C527.836,979.012 523.836,983.012 518.903,983.012 C513.969,983.012 509.97,979.012 509.97,974.079 C509.97,969.145 513.969,965.146 518.903,965.146 C523.836,965.146 527.836,969.145 527.836,974.079" id="Fill-273" sketch:type="MSShapeGroup"></path><path d="M527.836,1000.88 C527.836,1005.81 523.836,1009.81 518.903,1009.81 C513.969,1009.81 509.97,1005.81 509.97,1000.88 C509.97,995.944 513.969,991.944 518.903,991.944 C523.836,991.944 527.836,995.944 527.836,1000.88" id="Fill-274" sketch:type="MSShapeGroup"></path><path d="M527.836,1027.68 C527.836,1032.61 523.836,1036.61 518.903,1036.61 C513.969,1036.61 509.97,1032.61 509.97,1027.68 C509.97,1022.74 513.969,1018.74 518.903,1018.74 C523.836,1018.74 527.836,1022.74 527.836,1027.68" id="Fill-275" sketch:type="MSShapeGroup"></path><path d="M527.836,1054.47 C527.836,1059.41 523.836,1063.41 518.903,1063.41 C513.969,1063.41 509.97,1059.41 509.97,1054.47 C509.97,1049.54 513.969,1045.54 518.903,1045.54 C523.836,1045.54 527.836,1049.54 527.836,1054.47" id="Fill-276" sketch:type="MSShapeGroup"></path><path d="M554.634,62.93 C554.634,67.863 550.635,71.863 545.701,71.863 C540.768,71.863 536.768,67.863 536.768,62.93 C536.768,57.996 540.768,53.997 545.701,53.997 C550.635,53.997 554.634,57.996 554.634,62.93" id="Fill-281" sketch:type="MSShapeGroup"></path><path d="M554.634,89.729 C554.634,94.662 550.635,98.661 545.701,98.661 C540.768,98.661 536.768,94.662 536.768,89.729 C536.768,84.795 540.768,80.796 545.701,80.796 C550.635,80.796 554.634,84.795 554.634,89.729" id="Fill-282" sketch:type="MSShapeGroup"></path><path d="M554.634,170.124 C554.634,175.057 550.635,179.057 545.701,179.057 C540.768,179.057 536.768,175.057 536.768,170.124 C536.768,165.19 540.768,161.191 545.701,161.191 C550.635,161.191 554.634,165.19 554.634,170.124" id="Fill-283" sketch:type="MSShapeGroup"></path><path d="M554.634,196.922 C554.634,201.856 550.635,205.855 545.701,205.855 C540.768,205.855 536.768,201.856 536.768,196.922 C536.768,191.989 540.768,187.989 545.701,187.989 C550.635,187.989 554.634,191.989 554.634,196.922" id="Fill-284" sketch:type="MSShapeGroup"></path><path d="M554.634,223.721 C554.634,228.654 550.635,232.654 545.701,232.654 C540.768,232.654 536.768,228.654 536.768,223.721 C536.768,218.787 540.768,214.788 545.701,214.788 C550.635,214.788 554.634,218.787 554.634,223.721" id="Fill-285" sketch:type="MSShapeGroup"></path><path d="M554.634,250.519 C554.634,255.453 550.635,259.452 545.701,259.452 C540.768,259.452 536.768,255.453 536.768,250.519 C536.768,245.586 540.768,241.586 545.701,241.586 C550.635,241.586 554.634,245.586 554.634,250.519" id="Fill-286" sketch:type="MSShapeGroup"></path><path d="M554.634,277.318 C554.634,282.251 550.635,286.251 545.701,286.251 C540.768,286.251 536.768,282.251 536.768,277.318 C536.768,272.384 540.768,268.385 545.701,268.385 C550.635,268.385 554.634,272.384 554.634,277.318" id="Fill-287" sketch:type="MSShapeGroup"></path><path d="M554.634,304.116 C554.634,309.05 550.635,313.049 545.701,313.049 C540.768,313.049 536.768,309.05 536.768,304.116 C536.768,299.183 540.768,295.184 545.701,295.184 C550.635,295.184 554.634,299.183 554.634,304.116" id="Fill-288" sketch:type="MSShapeGroup"></path><path d="M554.634,330.915 C554.634,335.848 550.635,339.848 545.701,339.848 C540.768,339.848 536.768,335.848 536.768,330.915 C536.768,325.981 540.768,321.982 545.701,321.982 C550.635,321.982 554.634,325.981 554.634,330.915" id="Fill-289" sketch:type="MSShapeGroup"></path><path d="M554.634,357.713 C554.634,362.647 550.635,366.646 545.701,366.646 C540.768,366.646 536.768,362.647 536.768,357.713 C536.768,352.78 540.768,348.78 545.701,348.78 C550.635,348.78 554.634,352.78 554.634,357.713" id="Fill-290" sketch:type="MSShapeGroup"></path><path d="M554.634,384.512 C554.634,389.445 550.635,393.445 545.701,393.445 C540.768,393.445 536.768,389.445 536.768,384.512 C536.768,379.578 540.768,375.579 545.701,375.579 C550.635,375.579 554.634,379.578 554.634,384.512" id="Fill-291" sketch:type="MSShapeGroup"></path><path d="M554.634,411.31 C554.634,416.244 550.635,420.243 545.701,420.243 C540.768,420.243 536.768,416.244 536.768,411.31 C536.768,406.377 540.768,402.378 545.701,402.378 C550.635,402.378 554.634,406.377 554.634,411.31" id="Fill-292" sketch:type="MSShapeGroup"></path><path d="M554.634,438.109 C554.634,443.042 550.635,447.042 545.701,447.042 C540.768,447.042 536.768,443.042 536.768,438.109 C536.768,433.175 540.768,429.176 545.701,429.176 C550.635,429.176 554.634,433.175 554.634,438.109" id="Fill-293" sketch:type="MSShapeGroup"></path><path d="M554.634,464.907 C554.634,469.841 550.635,473.84 545.701,473.84 C540.768,473.84 536.768,469.841 536.768,464.907 C536.768,459.974 540.768,455.975 545.701,455.975 C550.635,455.975 554.634,459.974 554.634,464.907" id="Fill-294" sketch:type="MSShapeGroup"></path><path d="M554.634,491.706 C554.634,496.639 550.635,500.639 545.701,500.639 C540.768,500.639 536.768,496.639 536.768,491.706 C536.768,486.772 540.768,482.773 545.701,482.773 C550.635,482.773 554.634,486.772 554.634,491.706" id="Fill-295" sketch:type="MSShapeGroup"></path><path d="M554.634,598.9 C554.634,603.833 550.635,607.833 545.701,607.833 C540.768,607.833 536.768,603.833 536.768,598.9 C536.768,593.966 540.768,589.967 545.701,589.967 C550.635,589.967 554.634,593.966 554.634,598.9" id="Fill-296" sketch:type="MSShapeGroup"></path><path d="M554.634,679.295 C554.634,684.229 550.635,688.228 545.701,688.228 C540.768,688.228 536.768,684.229 536.768,679.295 C536.768,674.362 540.768,670.363 545.701,670.363 C550.635,670.363 554.634,674.362 554.634,679.295" id="Fill-297" sketch:type="MSShapeGroup"></path><path d="M554.634,706.094 C554.634,711.027 550.635,715.027 545.701,715.027 C540.768,715.027 536.768,711.027 536.768,706.094 C536.768,701.16 540.768,697.161 545.701,697.161 C550.635,697.161 554.634,701.16 554.634,706.094" id="Fill-298" sketch:type="MSShapeGroup"></path><path d="M554.634,732.892 C554.634,737.826 550.635,741.825 545.701,741.825 C540.768,741.825 536.768,737.826 536.768,732.892 C536.768,727.959 540.768,723.96 545.701,723.96 C550.635,723.96 554.634,727.959 554.634,732.892" id="Fill-299" sketch:type="MSShapeGroup"></path><path d="M554.634,759.691 C554.634,764.624 550.635,768.624 545.701,768.624 C540.768,768.624 536.768,764.624 536.768,759.691 C536.768,754.757 540.768,750.758 545.701,750.758 C550.635,750.758 554.634,754.757 554.634,759.691" id="Fill-300" sketch:type="MSShapeGroup"></path><path d="M554.634,786.489 C554.634,791.423 550.635,795.422 545.701,795.422 C540.768,795.422 536.768,791.423 536.768,786.489 C536.768,781.556 540.768,777.556 545.701,777.556 C550.635,777.556 554.634,781.556 554.634,786.489" id="Fill-301" sketch:type="MSShapeGroup"></path><path d="M554.634,813.288 C554.634,818.221 550.635,822.221 545.701,822.221 C540.768,822.221 536.768,818.221 536.768,813.288 C536.768,808.354 540.768,804.355 545.701,804.355 C550.635,804.355 554.634,808.354 554.634,813.288" id="Fill-302" sketch:type="MSShapeGroup"></path><path d="M554.634,840.086 C554.634,845.02 550.635,849.019 545.701,849.019 C540.768,849.019 536.768,845.02 536.768,840.086 C536.768,835.153 540.768,831.154 545.701,831.154 C550.635,831.154 554.634,835.153 554.634,840.086" id="Fill-303" sketch:type="MSShapeGroup"></path><path d="M554.634,866.885 C554.634,871.818 550.635,875.818 545.701,875.818 C540.768,875.818 536.768,871.818 536.768,866.885 C536.768,861.951 540.768,857.952 545.701,857.952 C550.635,857.952 554.634,861.951 554.634,866.885" id="Fill-304" sketch:type="MSShapeGroup"></path><path d="M554.634,893.683 C554.634,898.617 550.635,902.616 545.701,902.616 C540.768,902.616 536.768,898.617 536.768,893.683 C536.768,888.75 540.768,884.751 545.701,884.751 C550.635,884.751 554.634,888.75 554.634,893.683" id="Fill-305" sketch:type="MSShapeGroup"></path><path d="M554.634,920.482 C554.634,925.415 550.635,929.415 545.701,929.415 C540.768,929.415 536.768,925.415 536.768,920.482 C536.768,915.548 540.768,911.549 545.701,911.549 C550.635,911.549 554.634,915.548 554.634,920.482" id="Fill-306" sketch:type="MSShapeGroup"></path><path d="M554.634,947.28 C554.634,952.214 550.635,956.213 545.701,956.213 C540.768,956.213 536.768,952.214 536.768,947.28 C536.768,942.347 540.768,938.348 545.701,938.348 C550.635,938.348 554.634,942.347 554.634,947.28" id="Fill-307" sketch:type="MSShapeGroup"></path><path d="M554.634,974.079 C554.634,979.012 550.635,983.012 545.701,983.012 C540.768,983.012 536.768,979.012 536.768,974.079 C536.768,969.145 540.768,965.146 545.701,965.146 C550.635,965.146 554.634,969.145 554.634,974.079" id="Fill-308" sketch:type="MSShapeGroup"></path><path d="M554.634,1000.88 C554.634,1005.81 550.635,1009.81 545.701,1009.81 C540.768,1009.81 536.768,1005.81 536.768,1000.88 C536.768,995.944 540.768,991.944 545.701,991.944 C550.635,991.944 554.634,995.944 554.634,1000.88" id="Fill-309" sketch:type="MSShapeGroup"></path><path d="M554.634,1027.68 C554.634,1032.61 550.635,1036.61 545.701,1036.61 C540.768,1036.61 536.768,1032.61 536.768,1027.68 C536.768,1022.74 540.768,1018.74 545.701,1018.74 C550.635,1018.74 554.634,1022.74 554.634,1027.68" id="Fill-310" sketch:type="MSShapeGroup"></path><path d="M554.634,1054.47 C554.634,1059.41 550.635,1063.41 545.701,1063.41 C540.768,1063.41 536.768,1059.41 536.768,1054.47 C536.768,1049.54 540.768,1045.54 545.701,1045.54 C550.635,1045.54 554.634,1049.54 554.634,1054.47" id="Fill-311" sketch:type="MSShapeGroup"></path><path d="M554.634,1081.27 C554.634,1086.21 550.635,1090.21 545.701,1090.21 C540.768,1090.21 536.768,1086.21 536.768,1081.27 C536.768,1076.34 540.768,1072.34 545.701,1072.34 C550.635,1072.34 554.634,1076.34 554.634,1081.27" id="Fill-312" sketch:type="MSShapeGroup"></path><path d="M581.433,62.93 C581.433,67.863 577.433,71.863 572.5,71.863 C567.566,71.863 563.567,67.863 563.567,62.93 C563.567,57.996 567.566,53.997 572.5,53.997 C577.433,53.997 581.433,57.996 581.433,62.93" id="Fill-313" sketch:type="MSShapeGroup"></path><path d="M581.433,89.729 C581.433,94.662 577.433,98.661 572.5,98.661 C567.566,98.661 563.567,94.662 563.567,89.729 C563.567,84.795 567.566,80.796 572.5,80.796 C577.433,80.796 581.433,84.795 581.433,89.729" id="Fill-314" sketch:type="MSShapeGroup"></path><path d="M581.433,170.124 C581.433,175.057 577.433,179.057 572.5,179.057 C567.566,179.057 563.567,175.057 563.567,170.124 C563.567,165.19 567.566,161.191 572.5,161.191 C577.433,161.191 581.433,165.19 581.433,170.124" id="Fill-315" sketch:type="MSShapeGroup"></path><path d="M581.433,196.922 C581.433,201.856 577.433,205.855 572.5,205.855 C567.566,205.855 563.567,201.856 563.567,196.922 C563.567,191.989 567.566,187.989 572.5,187.989 C577.433,187.989 581.433,191.989 581.433,196.922" id="Fill-316" sketch:type="MSShapeGroup"></path><path d="M581.433,223.721 C581.433,228.654 577.433,232.654 572.5,232.654 C567.566,232.654 563.567,228.654 563.567,223.721 C563.567,218.787 567.566,214.788 572.5,214.788 C577.433,214.788 581.433,218.787 581.433,223.721" id="Fill-317" sketch:type="MSShapeGroup"></path><path d="M581.433,250.519 C581.433,255.453 577.433,259.452 572.5,259.452 C567.566,259.452 563.567,255.453 563.567,250.519 C563.567,245.586 567.566,241.586 572.5,241.586 C577.433,241.586 581.433,245.586 581.433,250.519" id="Fill-318" sketch:type="MSShapeGroup"></path><path d="M581.433,277.318 C581.433,282.251 577.433,286.251 572.5,286.251 C567.566,286.251 563.567,282.251 563.567,277.318 C563.567,272.384 567.566,268.385 572.5,268.385 C577.433,268.385 581.433,272.384 581.433,277.318" id="Fill-319" sketch:type="MSShapeGroup"></path><path d="M581.433,304.116 C581.433,309.05 577.433,313.049 572.5,313.049 C567.566,313.049 563.567,309.05 563.567,304.116 C563.567,299.183 567.566,295.184 572.5,295.184 C577.433,295.184 581.433,299.183 581.433,304.116" id="Fill-320" sketch:type="MSShapeGroup"></path><path d="M581.433,330.915 C581.433,335.848 577.433,339.848 572.5,339.848 C567.566,339.848 563.567,335.848 563.567,330.915 C563.567,325.981 567.566,321.982 572.5,321.982 C577.433,321.982 581.433,325.981 581.433,330.915" id="Fill-321" sketch:type="MSShapeGroup"></path><path d="M581.433,357.713 C581.433,362.647 577.433,366.646 572.5,366.646 C567.566,366.646 563.567,362.647 563.567,357.713 C563.567,352.78 567.566,348.78 572.5,348.78 C577.433,348.78 581.433,352.78 581.433,357.713" id="Fill-322" sketch:type="MSShapeGroup"></path><path d="M581.433,384.512 C581.433,389.445 577.433,393.445 572.5,393.445 C567.566,393.445 563.567,389.445 563.567,384.512 C563.567,379.578 567.566,375.579 572.5,375.579 C577.433,375.579 581.433,379.578 581.433,384.512" id="Fill-323" sketch:type="MSShapeGroup"></path><path d="M581.433,411.31 C581.433,416.244 577.433,420.243 572.5,420.243 C567.566,420.243 563.567,416.244 563.567,411.31 C563.567,406.377 567.566,402.378 572.5,402.378 C577.433,402.378 581.433,406.377 581.433,411.31" id="Fill-324" sketch:type="MSShapeGroup"></path><path d="M581.433,679.295 C581.433,684.229 577.433,688.228 572.5,688.228 C567.566,688.228 563.567,684.229 563.567,679.295 C563.567,674.362 567.566,670.363 572.5,670.363 C577.433,670.363 581.433,674.362 581.433,679.295" id="Fill-325" sketch:type="MSShapeGroup"></path><path d="M581.433,706.094 C581.433,711.027 577.433,715.027 572.5,715.027 C567.566,715.027 563.567,711.027 563.567,706.094 C563.567,701.16 567.566,697.161 572.5,697.161 C577.433,697.161 581.433,701.16 581.433,706.094" id="Fill-326" sketch:type="MSShapeGroup"></path><path d="M581.433,732.892 C581.433,737.826 577.433,741.825 572.5,741.825 C567.566,741.825 563.567,737.826 563.567,732.892 C563.567,727.959 567.566,723.96 572.5,723.96 C577.433,723.96 581.433,727.959 581.433,732.892" id="Fill-327" sketch:type="MSShapeGroup"></path><path d="M581.433,759.691 C581.433,764.624 577.433,768.624 572.5,768.624 C567.566,768.624 563.567,764.624 563.567,759.691 C563.567,754.757 567.566,750.758 572.5,750.758 C577.433,750.758 581.433,754.757 581.433,759.691" id="Fill-328" sketch:type="MSShapeGroup"></path><path d="M581.433,786.489 C581.433,791.423 577.433,795.422 572.5,795.422 C567.566,795.422 563.567,791.423 563.567,786.489 C563.567,781.556 567.566,777.556 572.5,777.556 C577.433,777.556 581.433,781.556 581.433,786.489" id="Fill-329" sketch:type="MSShapeGroup"></path><path d="M581.433,813.288 C581.433,818.221 577.433,822.221 572.5,822.221 C567.566,822.221 563.567,818.221 563.567,813.288 C563.567,808.354 567.566,804.355 572.5,804.355 C577.433,804.355 581.433,808.354 581.433,813.288" id="Fill-330" sketch:type="MSShapeGroup"></path><path d="M581.433,840.086 C581.433,845.02 577.433,849.019 572.5,849.019 C567.566,849.019 563.567,845.02 563.567,840.086 C563.567,835.153 567.566,831.154 572.5,831.154 C577.433,831.154 581.433,835.153 581.433,840.086" id="Fill-331" sketch:type="MSShapeGroup"></path><path d="M581.433,866.885 C581.433,871.818 577.433,875.818 572.5,875.818 C567.566,875.818 563.567,871.818 563.567,866.885 C563.567,861.951 567.566,857.952 572.5,857.952 C577.433,857.952 581.433,861.951 581.433,866.885" id="Fill-332" sketch:type="MSShapeGroup"></path><path d="M581.433,893.683 C581.433,898.617 577.433,902.616 572.5,902.616 C567.566,902.616 563.567,898.617 563.567,893.683 C563.567,888.75 567.566,884.751 572.5,884.751 C577.433,884.751 581.433,888.75 581.433,893.683" id="Fill-333" sketch:type="MSShapeGroup"></path><path d="M581.433,920.482 C581.433,925.415 577.433,929.415 572.5,929.415 C567.566,929.415 563.567,925.415 563.567,920.482 C563.567,915.548 567.566,911.549 572.5,911.549 C577.433,911.549 581.433,915.548 581.433,920.482" id="Fill-334" sketch:type="MSShapeGroup"></path><path d="M581.433,947.28 C581.433,952.214 577.433,956.213 572.5,956.213 C567.566,956.213 563.567,952.214 563.567,947.28 C563.567,942.347 567.566,938.348 572.5,938.348 C577.433,938.348 581.433,942.347 581.433,947.28" id="Fill-335" sketch:type="MSShapeGroup"></path><path d="M581.433,974.079 C581.433,979.012 577.433,983.012 572.5,983.012 C567.566,983.012 563.567,979.012 563.567,974.079 C563.567,969.145 567.566,965.146 572.5,965.146 C577.433,965.146 581.433,969.145 581.433,974.079" id="Fill-336" sketch:type="MSShapeGroup"></path><path d="M581.433,1000.88 C581.433,1005.81 577.433,1009.81 572.5,1009.81 C567.566,1009.81 563.567,1005.81 563.567,1000.88 C563.567,995.944 567.566,991.944 572.5,991.944 C577.433,991.944 581.433,995.944 581.433,1000.88" id="Fill-337" sketch:type="MSShapeGroup"></path><path d="M608.231,36.131 C608.231,41.065 604.232,45.064 599.298,45.064 C594.365,45.064 590.365,41.065 590.365,36.131 C590.365,31.198 594.365,27.198 599.298,27.198 C604.232,27.198 608.231,31.198 608.231,36.131" id="Fill-338" sketch:type="MSShapeGroup"></path><path d="M608.231,62.93 C608.231,67.863 604.232,71.863 599.298,71.863 C594.365,71.863 590.365,67.863 590.365,62.93 C590.365,57.996 594.365,53.997 599.298,53.997 C604.232,53.997 608.231,57.996 608.231,62.93" id="Fill-339" sketch:type="MSShapeGroup"></path><path d="M608.231,89.729 C608.231,94.662 604.232,98.661 599.298,98.661 C594.365,98.661 590.365,94.662 590.365,89.729 C590.365,84.795 594.365,80.796 599.298,80.796 C604.232,80.796 608.231,84.795 608.231,89.729" id="Fill-340" sketch:type="MSShapeGroup"></path><path d="M608.231,223.721 C608.231,228.654 604.232,232.654 599.298,232.654 C594.365,232.654 590.365,228.654 590.365,223.721 C590.365,218.787 594.365,214.788 599.298,214.788 C604.232,214.788 608.231,218.787 608.231,223.721" id="Fill-341" sketch:type="MSShapeGroup"></path><path d="M608.231,250.519 C608.231,255.453 604.232,259.452 599.298,259.452 C594.365,259.452 590.365,255.453 590.365,250.519 C590.365,245.586 594.365,241.586 599.298,241.586 C604.232,241.586 608.231,245.586 608.231,250.519" id="Fill-342" sketch:type="MSShapeGroup"></path><path d="M608.231,330.915 C608.231,335.848 604.232,339.848 599.298,339.848 C594.365,339.848 590.365,335.848 590.365,330.915 C590.365,325.981 594.365,321.982 599.298,321.982 C604.232,321.982 608.231,325.981 608.231,330.915" id="Fill-343" sketch:type="MSShapeGroup"></path><path d="M608.231,357.713 C608.231,362.647 604.232,366.646 599.298,366.646 C594.365,366.646 590.365,362.647 590.365,357.713 C590.365,352.78 594.365,348.78 599.298,348.78 C604.232,348.78 608.231,352.78 608.231,357.713" id="Fill-344" sketch:type="MSShapeGroup"></path><path d="M608.231,384.512 C608.231,389.445 604.232,393.445 599.298,393.445 C594.365,393.445 590.365,389.445 590.365,384.512 C590.365,379.578 594.365,375.579 599.298,375.579 C604.232,375.579 608.231,379.578 608.231,384.512" id="Fill-345" sketch:type="MSShapeGroup"></path><path d="M608.231,411.31 C608.231,416.244 604.232,420.243 599.298,420.243 C594.365,420.243 590.365,416.244 590.365,411.31 C590.365,406.377 594.365,402.378 599.298,402.378 C604.232,402.378 608.231,406.377 608.231,411.31" id="Fill-346" sketch:type="MSShapeGroup"></path><path d="M608.231,706.094 C608.231,711.027 604.232,715.027 599.298,715.027 C594.365,715.027 590.365,711.027 590.365,706.094 C590.365,701.16 594.365,697.161 599.298,697.161 C604.232,697.161 608.231,701.16 608.231,706.094" id="Fill-347" sketch:type="MSShapeGroup"></path><path d="M608.231,732.892 C608.231,737.826 604.232,741.825 599.298,741.825 C594.365,741.825 590.365,737.826 590.365,732.892 C590.365,727.959 594.365,723.96 599.298,723.96 C604.232,723.96 608.231,727.959 608.231,732.892" id="Fill-348" sketch:type="MSShapeGroup"></path><path d="M608.231,759.691 C608.231,764.624 604.232,768.624 599.298,768.624 C594.365,768.624 590.365,764.624 590.365,759.691 C590.365,754.757 594.365,750.758 599.298,750.758 C604.232,750.758 608.231,754.757 608.231,759.691" id="Fill-349" sketch:type="MSShapeGroup"></path><path d="M608.231,786.489 C608.231,791.423 604.232,795.422 599.298,795.422 C594.365,795.422 590.365,791.423 590.365,786.489 C590.365,781.556 594.365,777.556 599.298,777.556 C604.232,777.556 608.231,781.556 608.231,786.489" id="Fill-350" sketch:type="MSShapeGroup"></path><path d="M608.231,813.288 C608.231,818.221 604.232,822.221 599.298,822.221 C594.365,822.221 590.365,818.221 590.365,813.288 C590.365,808.354 594.365,804.355 599.298,804.355 C604.232,804.355 608.231,808.354 608.231,813.288" id="Fill-351" sketch:type="MSShapeGroup"></path><path d="M608.231,840.086 C608.231,845.02 604.232,849.019 599.298,849.019 C594.365,849.019 590.365,845.02 590.365,840.086 C590.365,835.153 594.365,831.154 599.298,831.154 C604.232,831.154 608.231,835.153 608.231,840.086" id="Fill-352" sketch:type="MSShapeGroup"></path><path d="M608.231,866.885 C608.231,871.818 604.232,875.818 599.298,875.818 C594.365,875.818 590.365,871.818 590.365,866.885 C590.365,861.951 594.365,857.952 599.298,857.952 C604.232,857.952 608.231,861.951 608.231,866.885" id="Fill-353" sketch:type="MSShapeGroup"></path><path d="M608.231,893.683 C608.231,898.617 604.232,902.616 599.298,902.616 C594.365,902.616 590.365,898.617 590.365,893.683 C590.365,888.75 594.365,884.751 599.298,884.751 C604.232,884.751 608.231,888.75 608.231,893.683" id="Fill-354" sketch:type="MSShapeGroup"></path><path d="M608.231,920.482 C608.231,925.415 604.232,929.415 599.298,929.415 C594.365,929.415 590.365,925.415 590.365,920.482 C590.365,915.548 594.365,911.549 599.298,911.549 C604.232,911.549 608.231,915.548 608.231,920.482" id="Fill-355" sketch:type="MSShapeGroup"></path><path d="M608.231,947.28 C608.231,952.214 604.232,956.213 599.298,956.213 C594.365,956.213 590.365,952.214 590.365,947.28 C590.365,942.347 594.365,938.348 599.298,938.348 C604.232,938.348 608.231,942.347 608.231,947.28" id="Fill-356" sketch:type="MSShapeGroup"></path><path d="M608.231,974.079 C608.231,979.012 604.232,983.012 599.298,983.012 C594.365,983.012 590.365,979.012 590.365,974.079 C590.365,969.145 594.365,965.146 599.298,965.146 C604.232,965.146 608.231,969.145 608.231,974.079" id="Fill-357" sketch:type="MSShapeGroup"></path><path d="M608.231,1000.88 C608.231,1005.81 604.232,1009.81 599.298,1009.81 C594.365,1009.81 590.365,1005.81 590.365,1000.88 C590.365,995.944 594.365,991.944 599.298,991.944 C604.232,991.944 608.231,995.944 608.231,1000.88" id="Fill-358" sketch:type="MSShapeGroup"></path><path d="M635.03,36.131 C635.03,41.065 631.03,45.064 626.097,45.064 C621.163,45.064 617.164,41.065 617.164,36.131 C617.164,31.198 621.163,27.198 626.097,27.198 C631.03,27.198 635.03,31.198 635.03,36.131" id="Fill-359" sketch:type="MSShapeGroup"></path><path d="M635.03,62.93 C635.03,67.863 631.03,71.863 626.097,71.863 C621.163,71.863 617.164,67.863 617.164,62.93 C617.164,57.996 621.163,53.997 626.097,53.997 C631.03,53.997 635.03,57.996 635.03,62.93" id="Fill-360" sketch:type="MSShapeGroup"></path><path d="M635.03,89.729 C635.03,94.662 631.03,98.661 626.097,98.661 C621.163,98.661 617.164,94.662 617.164,89.729 C617.164,84.795 621.163,80.796 626.097,80.796 C631.03,80.796 635.03,84.795 635.03,89.729" id="Fill-361" sketch:type="MSShapeGroup"></path><path d="M635.03,116.527 C635.03,121.46 631.03,125.46 626.097,125.46 C621.163,125.46 617.164,121.46 617.164,116.527 C617.164,111.593 621.163,107.594 626.097,107.594 C631.03,107.594 635.03,111.593 635.03,116.527" id="Fill-362" sketch:type="MSShapeGroup"></path><path d="M635.03,143.325 C635.03,148.259 631.03,152.258 626.097,152.258 C621.163,152.258 617.164,148.259 617.164,143.325 C617.164,138.392 621.163,134.392 626.097,134.392 C631.03,134.392 635.03,138.392 635.03,143.325" id="Fill-363" sketch:type="MSShapeGroup"></path><path d="M635.03,170.124 C635.03,175.057 631.03,179.057 626.097,179.057 C621.163,179.057 617.164,175.057 617.164,170.124 C617.164,165.19 621.163,161.191 626.097,161.191 C631.03,161.191 635.03,165.19 635.03,170.124" id="Fill-364" sketch:type="MSShapeGroup"></path><path d="M635.03,357.713 C635.03,362.647 631.03,366.646 626.097,366.646 C621.163,366.646 617.164,362.647 617.164,357.713 C617.164,352.78 621.163,348.78 626.097,348.78 C631.03,348.78 635.03,352.78 635.03,357.713" id="Fill-365" sketch:type="MSShapeGroup"></path><path d="M635.03,384.512 C635.03,389.445 631.03,393.445 626.097,393.445 C621.163,393.445 617.164,389.445 617.164,384.512 C617.164,379.578 621.163,375.579 626.097,375.579 C631.03,375.579 635.03,379.578 635.03,384.512" id="Fill-366" sketch:type="MSShapeGroup"></path><path d="M635.03,411.31 C635.03,416.244 631.03,420.243 626.097,420.243 C621.163,420.243 617.164,416.244 617.164,411.31 C617.164,406.377 621.163,402.378 626.097,402.378 C631.03,402.378 635.03,406.377 635.03,411.31" id="Fill-367" sketch:type="MSShapeGroup"></path><path d="M635.03,706.094 C635.03,711.027 631.03,715.027 626.097,715.027 C621.163,715.027 617.164,711.027 617.164,706.094 C617.164,701.16 621.163,697.161 626.097,697.161 C631.03,697.161 635.03,701.16 635.03,706.094" id="Fill-368" sketch:type="MSShapeGroup"></path><path d="M635.03,732.892 C635.03,737.826 631.03,741.825 626.097,741.825 C621.163,741.825 617.164,737.826 617.164,732.892 C617.164,727.959 621.163,723.96 626.097,723.96 C631.03,723.96 635.03,727.959 635.03,732.892" id="Fill-369" sketch:type="MSShapeGroup"></path><path d="M635.03,759.691 C635.03,764.624 631.03,768.624 626.097,768.624 C621.163,768.624 617.164,764.624 617.164,759.691 C617.164,754.757 621.163,750.758 626.097,750.758 C631.03,750.758 635.03,754.757 635.03,759.691" id="Fill-370" sketch:type="MSShapeGroup"></path><path d="M635.03,786.489 C635.03,791.423 631.03,795.422 626.097,795.422 C621.163,795.422 617.164,791.423 617.164,786.489 C617.164,781.556 621.163,777.556 626.097,777.556 C631.03,777.556 635.03,781.556 635.03,786.489" id="Fill-371" sketch:type="MSShapeGroup"></path><path d="M635.03,813.288 C635.03,818.221 631.03,822.221 626.097,822.221 C621.163,822.221 617.164,818.221 617.164,813.288 C617.164,808.354 621.163,804.355 626.097,804.355 C631.03,804.355 635.03,808.354 635.03,813.288" id="Fill-372" sketch:type="MSShapeGroup"></path><path d="M635.03,840.086 C635.03,845.02 631.03,849.019 626.097,849.019 C621.163,849.019 617.164,845.02 617.164,840.086 C617.164,835.153 621.163,831.154 626.097,831.154 C631.03,831.154 635.03,835.153 635.03,840.086" id="Fill-373" sketch:type="MSShapeGroup"></path><path d="M635.03,866.885 C635.03,871.818 631.03,875.818 626.097,875.818 C621.163,875.818 617.164,871.818 617.164,866.885 C617.164,861.951 621.163,857.952 626.097,857.952 C631.03,857.952 635.03,861.951 635.03,866.885" id="Fill-374" sketch:type="MSShapeGroup"></path><path d="M635.03,893.683 C635.03,898.617 631.03,902.616 626.097,902.616 C621.163,902.616 617.164,898.617 617.164,893.683 C617.164,888.75 621.163,884.751 626.097,884.751 C631.03,884.751 635.03,888.75 635.03,893.683" id="Fill-375" sketch:type="MSShapeGroup"></path><path d="M635.03,920.482 C635.03,925.415 631.03,929.415 626.097,929.415 C621.163,929.415 617.164,925.415 617.164,920.482 C617.164,915.548 621.163,911.549 626.097,911.549 C631.03,911.549 635.03,915.548 635.03,920.482" id="Fill-376" sketch:type="MSShapeGroup"></path><path d="M635.03,947.28 C635.03,952.214 631.03,956.213 626.097,956.213 C621.163,956.213 617.164,952.214 617.164,947.28 C617.164,942.347 621.163,938.348 626.097,938.348 C631.03,938.348 635.03,942.347 635.03,947.28" id="Fill-377" sketch:type="MSShapeGroup"></path><path d="M661.828,36.131 C661.828,41.065 657.829,45.064 652.895,45.064 C647.962,45.064 643.962,41.065 643.962,36.131 C643.962,31.198 647.962,27.198 652.895,27.198 C657.829,27.198 661.828,31.198 661.828,36.131" id="Fill-378" sketch:type="MSShapeGroup"></path><path d="M661.828,62.93 C661.828,67.863 657.829,71.863 652.895,71.863 C647.962,71.863 643.962,67.863 643.962,62.93 C643.962,57.996 647.962,53.997 652.895,53.997 C657.829,53.997 661.828,57.996 661.828,62.93" id="Fill-379" sketch:type="MSShapeGroup"></path><path d="M661.828,89.729 C661.828,94.662 657.829,98.661 652.895,98.661 C647.962,98.661 643.962,94.662 643.962,89.729 C643.962,84.795 647.962,80.796 652.895,80.796 C657.829,80.796 661.828,84.795 661.828,89.729" id="Fill-380" sketch:type="MSShapeGroup"></path><path d="M661.828,116.527 C661.828,121.46 657.829,125.46 652.895,125.46 C647.962,125.46 643.962,121.46 643.962,116.527 C643.962,111.593 647.962,107.594 652.895,107.594 C657.829,107.594 661.828,111.593 661.828,116.527" id="Fill-381" sketch:type="MSShapeGroup"></path><path d="M661.828,143.325 C661.828,148.259 657.829,152.258 652.895,152.258 C647.962,152.258 643.962,148.259 643.962,143.325 C643.962,138.392 647.962,134.392 652.895,134.392 C657.829,134.392 661.828,138.392 661.828,143.325" id="Fill-382" sketch:type="MSShapeGroup"></path><path d="M661.828,170.124 C661.828,175.057 657.829,179.057 652.895,179.057 C647.962,179.057 643.962,175.057 643.962,170.124 C643.962,165.19 647.962,161.191 652.895,161.191 C657.829,161.191 661.828,165.19 661.828,170.124" id="Fill-383" sketch:type="MSShapeGroup"></path><path d="M661.828,196.922 C661.828,201.856 657.829,205.855 652.895,205.855 C647.962,205.855 643.962,201.856 643.962,196.922 C643.962,191.989 647.962,187.989 652.895,187.989 C657.829,187.989 661.828,191.989 661.828,196.922" id="Fill-384" sketch:type="MSShapeGroup"></path><path d="M661.828,223.721 C661.828,228.654 657.829,232.654 652.895,232.654 C647.962,232.654 643.962,228.654 643.962,223.721 C643.962,218.787 647.962,214.788 652.895,214.788 C657.829,214.788 661.828,218.787 661.828,223.721" id="Fill-385" sketch:type="MSShapeGroup"></path><path d="M661.828,250.519 C661.828,255.453 657.829,259.452 652.895,259.452 C647.962,259.452 643.962,255.453 643.962,250.519 C643.962,245.586 647.962,241.586 652.895,241.586 C657.829,241.586 661.828,245.586 661.828,250.519" id="Fill-386" sketch:type="MSShapeGroup"></path><path d="M661.828,732.892 C661.828,737.826 657.829,741.825 652.895,741.825 C647.962,741.825 643.962,737.826 643.962,732.892 C643.962,727.959 647.962,723.96 652.895,723.96 C657.829,723.96 661.828,727.959 661.828,732.892" id="Fill-387" sketch:type="MSShapeGroup"></path><path d="M661.828,759.691 C661.828,764.624 657.829,768.624 652.895,768.624 C647.962,768.624 643.962,764.624 643.962,759.691 C643.962,754.757 647.962,750.758 652.895,750.758 C657.829,750.758 661.828,754.757 661.828,759.691" id="Fill-388" sketch:type="MSShapeGroup"></path><path d="M661.828,786.489 C661.828,791.423 657.829,795.422 652.895,795.422 C647.962,795.422 643.962,791.423 643.962,786.489 C643.962,781.556 647.962,777.556 652.895,777.556 C657.829,777.556 661.828,781.556 661.828,786.489" id="Fill-389" sketch:type="MSShapeGroup"></path><path d="M661.828,813.288 C661.828,818.221 657.829,822.221 652.895,822.221 C647.962,822.221 643.962,818.221 643.962,813.288 C643.962,808.354 647.962,804.355 652.895,804.355 C657.829,804.355 661.828,808.354 661.828,813.288" id="Fill-390" sketch:type="MSShapeGroup"></path><path d="M661.828,840.086 C661.828,845.02 657.829,849.019 652.895,849.019 C647.962,849.019 643.962,845.02 643.962,840.086 C643.962,835.153 647.962,831.154 652.895,831.154 C657.829,831.154 661.828,835.153 661.828,840.086" id="Fill-391" sketch:type="MSShapeGroup"></path><path d="M661.828,866.885 C661.828,871.818 657.829,875.818 652.895,875.818 C647.962,875.818 643.962,871.818 643.962,866.885 C643.962,861.951 647.962,857.952 652.895,857.952 C657.829,857.952 661.828,861.951 661.828,866.885" id="Fill-392" sketch:type="MSShapeGroup"></path><path d="M661.828,893.683 C661.828,898.617 657.829,902.616 652.895,902.616 C647.962,902.616 643.962,898.617 643.962,893.683 C643.962,888.75 647.962,884.751 652.895,884.751 C657.829,884.751 661.828,888.75 661.828,893.683" id="Fill-393" sketch:type="MSShapeGroup"></path><path d="M661.828,920.482 C661.828,925.415 657.829,929.415 652.895,929.415 C647.962,929.415 643.962,925.415 643.962,920.482 C643.962,915.548 647.962,911.549 652.895,911.549 C657.829,911.549 661.828,915.548 661.828,920.482" id="Fill-394" sketch:type="MSShapeGroup"></path><path d="M661.828,947.28 C661.828,952.214 657.829,956.213 652.895,956.213 C647.962,956.213 643.962,952.214 643.962,947.28 C643.962,942.347 647.962,938.348 652.895,938.348 C657.829,938.348 661.828,942.347 661.828,947.28" id="Fill-395" sketch:type="MSShapeGroup"></path><path d="M688.627,9.333 C688.627,14.266 684.627,18.266 679.694,18.266 C674.76,18.266 670.761,14.266 670.761,9.333 C670.761,4.399 674.76,0.4 679.694,0.4 C684.627,0.4 688.627,4.399 688.627,9.333" id="Fill-396" sketch:type="MSShapeGroup"></path><path d="M688.627,36.131 C688.627,41.065 684.627,45.064 679.694,45.064 C674.76,45.064 670.761,41.065 670.761,36.131 C670.761,31.198 674.76,27.198 679.694,27.198 C684.627,27.198 688.627,31.198 688.627,36.131" id="Fill-397" sketch:type="MSShapeGroup"></path><path d="M688.627,62.93 C688.627,67.863 684.627,71.863 679.694,71.863 C674.76,71.863 670.761,67.863 670.761,62.93 C670.761,57.996 674.76,53.997 679.694,53.997 C684.627,53.997 688.627,57.996 688.627,62.93" id="Fill-398" sketch:type="MSShapeGroup"></path><path d="M688.627,89.729 C688.627,94.662 684.627,98.661 679.694,98.661 C674.76,98.661 670.761,94.662 670.761,89.729 C670.761,84.795 674.76,80.796 679.694,80.796 C684.627,80.796 688.627,84.795 688.627,89.729" id="Fill-399" sketch:type="MSShapeGroup"></path><path d="M688.627,116.527 C688.627,121.46 684.627,125.46 679.694,125.46 C674.76,125.46 670.761,121.46 670.761,116.527 C670.761,111.593 674.76,107.594 679.694,107.594 C684.627,107.594 688.627,111.593 688.627,116.527" id="Fill-400" sketch:type="MSShapeGroup"></path><path d="M688.627,143.325 C688.627,148.259 684.627,152.258 679.694,152.258 C674.76,152.258 670.761,148.259 670.761,143.325 C670.761,138.392 674.76,134.392 679.694,134.392 C684.627,134.392 688.627,138.392 688.627,143.325" id="Fill-401" sketch:type="MSShapeGroup"></path><path d="M688.627,170.124 C688.627,175.057 684.627,179.057 679.694,179.057 C674.76,179.057 670.761,175.057 670.761,170.124 C670.761,165.19 674.76,161.191 679.694,161.191 C684.627,161.191 688.627,165.19 688.627,170.124" id="Fill-402" sketch:type="MSShapeGroup"></path><path d="M688.627,196.922 C688.627,201.856 684.627,205.855 679.694,205.855 C674.76,205.855 670.761,201.856 670.761,196.922 C670.761,191.989 674.76,187.989 679.694,187.989 C684.627,187.989 688.627,191.989 688.627,196.922" id="Fill-403" sketch:type="MSShapeGroup"></path><path d="M688.627,223.721 C688.627,228.654 684.627,232.654 679.694,232.654 C674.76,232.654 670.761,228.654 670.761,223.721 C670.761,218.787 674.76,214.788 679.694,214.788 C684.627,214.788 688.627,218.787 688.627,223.721" id="Fill-404" sketch:type="MSShapeGroup"></path><path d="M688.627,250.519 C688.627,255.453 684.627,259.452 679.694,259.452 C674.76,259.452 670.761,255.453 670.761,250.519 C670.761,245.586 674.76,241.586 679.694,241.586 C684.627,241.586 688.627,245.586 688.627,250.519" id="Fill-405" sketch:type="MSShapeGroup"></path><path d="M688.627,277.318 C688.627,282.251 684.627,286.251 679.694,286.251 C674.76,286.251 670.761,282.251 670.761,277.318 C670.761,272.384 674.76,268.385 679.694,268.385 C684.627,268.385 688.627,272.384 688.627,277.318" id="Fill-406" sketch:type="MSShapeGroup"></path><path d="M688.627,759.691 C688.627,764.624 684.627,768.624 679.694,768.624 C674.76,768.624 670.761,764.624 670.761,759.691 C670.761,754.757 674.76,750.758 679.694,750.758 C684.627,750.758 688.627,754.757 688.627,759.691" id="Fill-407" sketch:type="MSShapeGroup"></path><path d="M688.627,786.489 C688.627,791.423 684.627,795.422 679.694,795.422 C674.76,795.422 670.761,791.423 670.761,786.489 C670.761,781.556 674.76,777.556 679.694,777.556 C684.627,777.556 688.627,781.556 688.627,786.489" id="Fill-408" sketch:type="MSShapeGroup"></path><path d="M688.627,813.288 C688.627,818.221 684.627,822.221 679.694,822.221 C674.76,822.221 670.761,818.221 670.761,813.288 C670.761,808.354 674.76,804.355 679.694,804.355 C684.627,804.355 688.627,808.354 688.627,813.288" id="Fill-409" sketch:type="MSShapeGroup"></path><path d="M688.627,840.086 C688.627,845.02 684.627,849.019 679.694,849.019 C674.76,849.019 670.761,845.02 670.761,840.086 C670.761,835.153 674.76,831.154 679.694,831.154 C684.627,831.154 688.627,835.153 688.627,840.086" id="Fill-410" sketch:type="MSShapeGroup"></path><path d="M688.627,866.885 C688.627,871.818 684.627,875.818 679.694,875.818 C674.76,875.818 670.761,871.818 670.761,866.885 C670.761,861.951 674.76,857.952 679.694,857.952 C684.627,857.952 688.627,861.951 688.627,866.885" id="Fill-411" sketch:type="MSShapeGroup"></path><path d="M688.627,893.683 C688.627,898.617 684.627,902.616 679.694,902.616 C674.76,902.616 670.761,898.617 670.761,893.683 C670.761,888.75 674.76,884.751 679.694,884.751 C684.627,884.751 688.627,888.75 688.627,893.683" id="Fill-412" sketch:type="MSShapeGroup"></path><path d="M715.425,9.333 C715.425,14.266 711.426,18.266 706.492,18.266 C701.559,18.266 697.559,14.266 697.559,9.333 C697.559,4.399 701.559,0.4 706.492,0.4 C711.426,0.4 715.425,4.399 715.425,9.333" id="Fill-413" sketch:type="MSShapeGroup"></path><path d="M715.425,36.131 C715.425,41.065 711.426,45.064 706.492,45.064 C701.559,45.064 697.559,41.065 697.559,36.131 C697.559,31.198 701.559,27.198 706.492,27.198 C711.426,27.198 715.425,31.198 715.425,36.131" id="Fill-414" sketch:type="MSShapeGroup"></path><path d="M715.425,62.93 C715.425,67.863 711.426,71.863 706.492,71.863 C701.559,71.863 697.559,67.863 697.559,62.93 C697.559,57.996 701.559,53.997 706.492,53.997 C711.426,53.997 715.425,57.996 715.425,62.93" id="Fill-415" sketch:type="MSShapeGroup"></path><path d="M715.425,89.729 C715.425,94.662 711.426,98.661 706.492,98.661 C701.559,98.661 697.559,94.662 697.559,89.729 C697.559,84.795 701.559,80.796 706.492,80.796 C711.426,80.796 715.425,84.795 715.425,89.729" id="Fill-416" sketch:type="MSShapeGroup"></path><path d="M715.425,116.527 C715.425,121.46 711.426,125.46 706.492,125.46 C701.559,125.46 697.559,121.46 697.559,116.527 C697.559,111.593 701.559,107.594 706.492,107.594 C711.426,107.594 715.425,111.593 715.425,116.527" id="Fill-417" sketch:type="MSShapeGroup"></path><path d="M715.425,143.325 C715.425,148.259 711.426,152.258 706.492,152.258 C701.559,152.258 697.559,148.259 697.559,143.325 C697.559,138.392 701.559,134.392 706.492,134.392 C711.426,134.392 715.425,138.392 715.425,143.325" id="Fill-418" sketch:type="MSShapeGroup"></path><path d="M715.425,170.124 C715.425,175.057 711.426,179.057 706.492,179.057 C701.559,179.057 697.559,175.057 697.559,170.124 C697.559,165.19 701.559,161.191 706.492,161.191 C711.426,161.191 715.425,165.19 715.425,170.124" id="Fill-419" sketch:type="MSShapeGroup"></path><path d="M715.425,196.922 C715.425,201.856 711.426,205.855 706.492,205.855 C701.559,205.855 697.559,201.856 697.559,196.922 C697.559,191.989 701.559,187.989 706.492,187.989 C711.426,187.989 715.425,191.989 715.425,196.922" id="Fill-420" sketch:type="MSShapeGroup"></path><path d="M715.425,223.721 C715.425,228.654 711.426,232.654 706.492,232.654 C701.559,232.654 697.559,228.654 697.559,223.721 C697.559,218.787 701.559,214.788 706.492,214.788 C711.426,214.788 715.425,218.787 715.425,223.721" id="Fill-421" sketch:type="MSShapeGroup"></path><path d="M715.425,250.519 C715.425,255.453 711.426,259.452 706.492,259.452 C701.559,259.452 697.559,255.453 697.559,250.519 C697.559,245.586 701.559,241.586 706.492,241.586 C711.426,241.586 715.425,245.586 715.425,250.519" id="Fill-422" sketch:type="MSShapeGroup"></path><path d="M715.425,277.318 C715.425,282.251 711.426,286.251 706.492,286.251 C701.559,286.251 697.559,282.251 697.559,277.318 C697.559,272.384 701.559,268.385 706.492,268.385 C711.426,268.385 715.425,272.384 715.425,277.318" id="Fill-423" sketch:type="MSShapeGroup"></path><path d="M715.425,759.691 C715.425,764.624 711.426,768.624 706.492,768.624 C701.559,768.624 697.559,764.624 697.559,759.691 C697.559,754.757 701.559,750.758 706.492,750.758 C711.426,750.758 715.425,754.757 715.425,759.691" id="Fill-424" sketch:type="MSShapeGroup"></path><path d="M715.425,786.489 C715.425,791.423 711.426,795.422 706.492,795.422 C701.559,795.422 697.559,791.423 697.559,786.489 C697.559,781.556 701.559,777.556 706.492,777.556 C711.426,777.556 715.425,781.556 715.425,786.489" id="Fill-425" sketch:type="MSShapeGroup"></path><path d="M715.425,813.288 C715.425,818.221 711.426,822.221 706.492,822.221 C701.559,822.221 697.559,818.221 697.559,813.288 C697.559,808.354 701.559,804.355 706.492,804.355 C711.426,804.355 715.425,808.354 715.425,813.288" id="Fill-426" sketch:type="MSShapeGroup"></path><path d="M715.425,840.086 C715.425,845.02 711.426,849.019 706.492,849.019 C701.559,849.019 697.559,845.02 697.559,840.086 C697.559,835.153 701.559,831.154 706.492,831.154 C711.426,831.154 715.425,835.153 715.425,840.086" id="Fill-427" sketch:type="MSShapeGroup"></path><path d="M715.425,866.885 C715.425,871.818 711.426,875.818 706.492,875.818 C701.559,875.818 697.559,871.818 697.559,866.885 C697.559,861.951 701.559,857.952 706.492,857.952 C711.426,857.952 715.425,861.951 715.425,866.885" id="Fill-428" sketch:type="MSShapeGroup"></path><path d="M742.224,36.131 C742.224,41.065 738.224,45.064 733.291,45.064 C728.357,45.064 724.358,41.065 724.358,36.131 C724.358,31.198 728.357,27.198 733.291,27.198 C738.224,27.198 742.224,31.198 742.224,36.131" id="Fill-429" sketch:type="MSShapeGroup"></path><path d="M742.224,62.93 C742.224,67.863 738.224,71.863 733.291,71.863 C728.357,71.863 724.358,67.863 724.358,62.93 C724.358,57.996 728.357,53.997 733.291,53.997 C738.224,53.997 742.224,57.996 742.224,62.93" id="Fill-430" sketch:type="MSShapeGroup"></path><path d="M742.224,89.729 C742.224,94.662 738.224,98.661 733.291,98.661 C728.357,98.661 724.358,94.662 724.358,89.729 C724.358,84.795 728.357,80.796 733.291,80.796 C738.224,80.796 742.224,84.795 742.224,89.729" id="Fill-431" sketch:type="MSShapeGroup"></path><path d="M742.224,116.527 C742.224,121.46 738.224,125.46 733.291,125.46 C728.357,125.46 724.358,121.46 724.358,116.527 C724.358,111.593 728.357,107.594 733.291,107.594 C738.224,107.594 742.224,111.593 742.224,116.527" id="Fill-432" sketch:type="MSShapeGroup"></path><path d="M742.224,143.325 C742.224,148.259 738.224,152.258 733.291,152.258 C728.357,152.258 724.358,148.259 724.358,143.325 C724.358,138.392 728.357,134.392 733.291,134.392 C738.224,134.392 742.224,138.392 742.224,143.325" id="Fill-433" sketch:type="MSShapeGroup"></path><path d="M742.224,170.124 C742.224,175.057 738.224,179.057 733.291,179.057 C728.357,179.057 724.358,175.057 724.358,170.124 C724.358,165.19 728.357,161.191 733.291,161.191 C738.224,161.191 742.224,165.19 742.224,170.124" id="Fill-434" sketch:type="MSShapeGroup"></path><path d="M742.224,196.922 C742.224,201.856 738.224,205.855 733.291,205.855 C728.357,205.855 724.358,201.856 724.358,196.922 C724.358,191.989 728.357,187.989 733.291,187.989 C738.224,187.989 742.224,191.989 742.224,196.922" id="Fill-435" sketch:type="MSShapeGroup"></path><path d="M742.224,223.721 C742.224,228.654 738.224,232.654 733.291,232.654 C728.357,232.654 724.358,228.654 724.358,223.721 C724.358,218.787 728.357,214.788 733.291,214.788 C738.224,214.788 742.224,218.787 742.224,223.721" id="Fill-436" sketch:type="MSShapeGroup"></path><path d="M742.224,786.489 C742.224,791.423 738.224,795.422 733.291,795.422 C728.357,795.422 724.358,791.423 724.358,786.489 C724.358,781.556 728.357,777.556 733.291,777.556 C738.224,777.556 742.224,781.556 742.224,786.489" id="Fill-437" sketch:type="MSShapeGroup"></path><path d="M769.022,62.93 C769.022,67.863 765.023,71.863 760.089,71.863 C755.156,71.863 751.156,67.863 751.156,62.93 C751.156,57.996 755.156,53.997 760.089,53.997 C765.023,53.997 769.022,57.996 769.022,62.93" id="Fill-438" sketch:type="MSShapeGroup"></path><path d="M769.022,89.729 C769.022,94.662 765.023,98.661 760.089,98.661 C755.156,98.661 751.156,94.662 751.156,89.729 C751.156,84.795 755.156,80.796 760.089,80.796 C765.023,80.796 769.022,84.795 769.022,89.729" id="Fill-439" sketch:type="MSShapeGroup"></path><path d="M769.022,116.527 C769.022,121.46 765.023,125.46 760.089,125.46 C755.156,125.46 751.156,121.46 751.156,116.527 C751.156,111.593 755.156,107.594 760.089,107.594 C765.023,107.594 769.022,111.593 769.022,116.527" id="Fill-440" sketch:type="MSShapeGroup"></path><path d="M769.022,143.325 C769.022,148.259 765.023,152.258 760.089,152.258 C755.156,152.258 751.156,148.259 751.156,143.325 C751.156,138.392 755.156,134.392 760.089,134.392 C765.023,134.392 769.022,138.392 769.022,143.325" id="Fill-441" sketch:type="MSShapeGroup"></path><path d="M769.022,170.124 C769.022,175.057 765.023,179.057 760.089,179.057 C755.156,179.057 751.156,175.057 751.156,170.124 C751.156,165.19 755.156,161.191 760.089,161.191 C765.023,161.191 769.022,165.19 769.022,170.124" id="Fill-442" sketch:type="MSShapeGroup"></path><path d="M769.022,196.922 C769.022,201.856 765.023,205.855 760.089,205.855 C755.156,205.855 751.156,201.856 751.156,196.922 C751.156,191.989 755.156,187.989 760.089,187.989 C765.023,187.989 769.022,191.989 769.022,196.922" id="Fill-443" sketch:type="MSShapeGroup"></path><path d="M795.821,89.729 C795.821,94.662 791.821,98.661 786.888,98.661 C781.954,98.661 777.955,94.662 777.955,89.729 C777.955,84.795 781.954,80.796 786.888,80.796 C791.821,80.796 795.821,84.795 795.821,89.729" id="Fill-444" sketch:type="MSShapeGroup"></path><path d="M795.821,116.527 C795.821,121.46 791.821,125.46 786.888,125.46 C781.954,125.46 777.955,121.46 777.955,116.527 C777.955,111.593 781.954,107.594 786.888,107.594 C791.821,107.594 795.821,111.593 795.821,116.527" id="Fill-445" sketch:type="MSShapeGroup"></path><path d="M795.821,143.325 C795.821,148.259 791.821,152.258 786.888,152.258 C781.954,152.258 777.955,148.259 777.955,143.325 C777.955,138.392 781.954,134.392 786.888,134.392 C791.821,134.392 795.821,138.392 795.821,143.325" id="Fill-446" sketch:type="MSShapeGroup"></path><path d="M795.821,170.124 C795.821,175.057 791.821,179.057 786.888,179.057 C781.954,179.057 777.955,175.057 777.955,170.124 C777.955,165.19 781.954,161.191 786.888,161.191 C791.821,161.191 795.821,165.19 795.821,170.124" id="Fill-447" sketch:type="MSShapeGroup"></path><path d="M795.821,196.922 C795.821,201.856 791.821,205.855 786.888,205.855 C781.954,205.855 777.955,201.856 777.955,196.922 C777.955,191.989 781.954,187.989 786.888,187.989 C791.821,187.989 795.821,191.989 795.821,196.922" id="Fill-448" sketch:type="MSShapeGroup"></path><path d="M795.821,223.721 C795.821,228.654 791.821,232.654 786.888,232.654 C781.954,232.654 777.955,228.654 777.955,223.721 C777.955,218.787 781.954,214.788 786.888,214.788 C791.821,214.788 795.821,218.787 795.821,223.721" id="Fill-449" sketch:type="MSShapeGroup"></path><path d="M822.619,89.729 C822.619,94.662 818.62,98.661 813.686,98.661 C808.753,98.661 804.753,94.662 804.753,89.729 C804.753,84.795 808.753,80.796 813.686,80.796 C818.62,80.796 822.619,84.795 822.619,89.729" id="Fill-450" sketch:type="MSShapeGroup"></path><path d="M822.619,116.527 C822.619,121.46 818.62,125.46 813.686,125.46 C808.753,125.46 804.753,121.46 804.753,116.527 C804.753,111.593 808.753,107.594 813.686,107.594 C818.62,107.594 822.619,111.593 822.619,116.527" id="Fill-451" sketch:type="MSShapeGroup"></path><path d="M822.619,143.325 C822.619,148.259 818.62,152.258 813.686,152.258 C808.753,152.258 804.753,148.259 804.753,143.325 C804.753,138.392 808.753,134.392 813.686,134.392 C818.62,134.392 822.619,138.392 822.619,143.325" id="Fill-452" sketch:type="MSShapeGroup"></path><path d="M822.619,170.124 C822.619,175.057 818.62,179.057 813.686,179.057 C808.753,179.057 804.753,175.057 804.753,170.124 C804.753,165.19 808.753,161.191 813.686,161.191 C818.62,161.191 822.619,165.19 822.619,170.124" id="Fill-453" sketch:type="MSShapeGroup"></path><path d="M822.619,196.922 C822.619,201.856 818.62,205.855 813.686,205.855 C808.753,205.855 804.753,201.856 804.753,196.922 C804.753,191.989 808.753,187.989 813.686,187.989 C818.62,187.989 822.619,191.989 822.619,196.922" id="Fill-454" sketch:type="MSShapeGroup"></path><path d="M822.619,223.721 C822.619,228.654 818.62,232.654 813.686,232.654 C808.753,232.654 804.753,228.654 804.753,223.721 C804.753,218.787 808.753,214.788 813.686,214.788 C818.62,214.788 822.619,218.787 822.619,223.721" id="Fill-455" sketch:type="MSShapeGroup"></path><path d="M849.418,196.922 C849.418,201.856 845.418,205.855 840.485,205.855 C835.551,205.855 831.552,201.856 831.552,196.922 C831.552,191.989 835.551,187.989 840.485,187.989 C845.418,187.989 849.418,191.989 849.418,196.922" id="Fill-456" sketch:type="MSShapeGroup"></path></g></g></g></svg>';
var mapRight = '<svg class="map map-right" width="1252px" height="957px" viewBox="0 0 1252 957" version="1.1" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:sketch="http://www.bohemiancoding.com/sketch/ns"><defs><linearGradient id="Gradient" x1="0%" y1="0%" x2="0%" y2="100%" gradientUnits="userSpaceOnUse">  <stop stop-color="#FFFFFF" offset="0%"></stop>  <stop stop-color="#B1FF87" offset="22.9970504%"></stop>  <stop stop-color="#30B096" offset="100%"></stop></linearGradient></defs><g id="Comp-3" stroke="none" stroke-width="1" fill="none" fill-rule="evenodd" sketch:type="MSPage"><g id="Artboard-13" sketch:type="MSArtboardGroup" transform="translate(-1964.000000, -920.000000)" fill="url(#Gradient)" ><g id="Word-Map-Right" sketch:type="MSLayerGroup" transform="translate(1964.000000, 920.000000)"  ><path d="M18.418,197.318 C18.418,202.251 14.418,206.251 9.485,206.251 C4.551,206.251 0.552,202.251 0.552,197.318 C0.552,192.384 4.551,188.385 9.485,188.385 C14.418,188.385 18.418,192.384 18.418,197.318" id="Fill-457" sketch:type="MSShapeGroup"></path><path d="M18.418,438.504 C18.418,443.438 14.418,447.437 9.485,447.437 C4.551,447.437 0.552,443.438 0.552,438.504 C0.552,433.571 4.551,429.572 9.485,429.572 C14.418,429.572 18.418,433.571 18.418,438.504" id="Fill-458" sketch:type="MSShapeGroup"></path><path d="M18.418,465.303 C18.418,470.236 14.418,474.236 9.485,474.236 C4.551,474.236 0.552,470.236 0.552,465.303 C0.552,460.369 4.551,456.37 9.485,456.37 C14.418,456.37 18.418,460.369 18.418,465.303" id="Fill-459" sketch:type="MSShapeGroup"></path><path d="M18.418,492.101 C18.418,497.035 14.418,501.034 9.485,501.034 C4.551,501.034 0.552,497.035 0.552,492.101 C0.552,487.168 4.551,483.168 9.485,483.168 C14.418,483.168 18.418,487.168 18.418,492.101" id="Fill-460" sketch:type="MSShapeGroup"></path><path d="M18.418,518.9 C18.418,523.833 14.418,527.833 9.485,527.833 C4.551,527.833 0.552,523.833 0.552,518.9 C0.552,513.966 4.551,509.967 9.485,509.967 C14.418,509.967 18.418,513.966 18.418,518.9" id="Fill-461" sketch:type="MSShapeGroup"></path><path d="M45.216,170.519 C45.216,175.453 41.217,179.452 36.283,179.452 C31.35,179.452 27.35,175.453 27.35,170.519 C27.35,165.586 31.35,161.586 36.283,161.586 C41.217,161.586 45.216,165.586 45.216,170.519" id="Fill-462" sketch:type="MSShapeGroup"></path><path d="M45.216,197.318 C45.216,202.251 41.217,206.251 36.283,206.251 C31.35,206.251 27.35,202.251 27.35,197.318 C27.35,192.384 31.35,188.385 36.283,188.385 C41.217,188.385 45.216,192.384 45.216,197.318" id="Fill-463" sketch:type="MSShapeGroup"></path><path d="M45.216,224.116 C45.216,229.05 41.217,233.049 36.283,233.049 C31.35,233.049 27.35,229.05 27.35,224.116 C27.35,219.183 31.35,215.184 36.283,215.184 C41.217,215.184 45.216,219.183 45.216,224.116" id="Fill-464" sketch:type="MSShapeGroup"></path><path d="M45.216,411.706 C45.216,416.639 41.217,420.639 36.283,420.639 C31.35,420.639 27.35,416.639 27.35,411.706 C27.35,406.772 31.35,402.773 36.283,402.773 C41.217,402.773 45.216,406.772 45.216,411.706" id="Fill-465" sketch:type="MSShapeGroup"></path><path d="M45.216,438.504 C45.216,443.438 41.217,447.437 36.283,447.437 C31.35,447.437 27.35,443.438 27.35,438.504 C27.35,433.571 31.35,429.572 36.283,429.572 C41.217,429.572 45.216,433.571 45.216,438.504" id="Fill-466" sketch:type="MSShapeGroup"></path><path d="M45.216,465.303 C45.216,470.236 41.217,474.236 36.283,474.236 C31.35,474.236 27.35,470.236 27.35,465.303 C27.35,460.369 31.35,456.37 36.283,456.37 C41.217,456.37 45.216,460.369 45.216,465.303" id="Fill-467" sketch:type="MSShapeGroup"></path><path d="M45.216,492.101 C45.216,497.035 41.217,501.034 36.283,501.034 C31.35,501.034 27.35,497.035 27.35,492.101 C27.35,487.168 31.35,483.168 36.283,483.168 C41.217,483.168 45.216,487.168 45.216,492.101" id="Fill-468" sketch:type="MSShapeGroup"></path><path d="M45.216,518.9 C45.216,523.833 41.217,527.833 36.283,527.833 C31.35,527.833 27.35,523.833 27.35,518.9 C27.35,513.966 31.35,509.967 36.283,509.967 C41.217,509.967 45.216,513.966 45.216,518.9" id="Fill-469" sketch:type="MSShapeGroup"></path><path d="M45.216,545.698 C45.216,550.632 41.217,554.631 36.283,554.631 C31.35,554.631 27.35,550.632 27.35,545.698 C27.35,540.765 31.35,536.766 36.283,536.766 C41.217,536.766 45.216,540.765 45.216,545.698" id="Fill-470" sketch:type="MSShapeGroup"></path><path d="M72.015,143.721 C72.015,148.654 68.015,152.654 63.082,152.654 C58.148,152.654 54.149,148.654 54.149,143.721 C54.149,138.787 58.148,134.788 63.082,134.788 C68.015,134.788 72.015,138.787 72.015,143.721" id="Fill-471" sketch:type="MSShapeGroup"></path><path d="M72.015,170.519 C72.015,175.453 68.015,179.452 63.082,179.452 C58.148,179.452 54.149,175.453 54.149,170.519 C54.149,165.586 58.148,161.586 63.082,161.586 C68.015,161.586 72.015,165.586 72.015,170.519" id="Fill-472" sketch:type="MSShapeGroup"></path><path d="M72.015,197.318 C72.015,202.251 68.015,206.251 63.082,206.251 C58.148,206.251 54.149,202.251 54.149,197.318 C54.149,192.384 58.148,188.385 63.082,188.385 C68.015,188.385 72.015,192.384 72.015,197.318" id="Fill-473" sketch:type="MSShapeGroup"></path><path d="M72.015,224.116 C72.015,229.05 68.015,233.049 63.082,233.049 C58.148,233.049 54.149,229.05 54.149,224.116 C54.149,219.183 58.148,215.184 63.082,215.184 C68.015,215.184 72.015,219.183 72.015,224.116" id="Fill-474" sketch:type="MSShapeGroup"></path><path d="M72.015,304.512 C72.015,309.445 68.015,313.445 63.082,313.445 C58.148,313.445 54.149,309.445 54.149,304.512 C54.149,299.578 58.148,295.579 63.082,295.579 C68.015,295.579 72.015,299.578 72.015,304.512" id="Fill-475" sketch:type="MSShapeGroup"></path><path d="M72.015,331.31 C72.015,336.244 68.015,340.243 63.082,340.243 C58.148,340.243 54.149,336.244 54.149,331.31 C54.149,326.377 58.148,322.378 63.082,322.378 C68.015,322.378 72.015,326.377 72.015,331.31" id="Fill-476" sketch:type="MSShapeGroup"></path><path d="M72.015,358.109 C72.015,363.042 68.015,367.042 63.082,367.042 C58.148,367.042 54.149,363.042 54.149,358.109 C54.149,353.175 58.148,349.176 63.082,349.176 C68.015,349.176 72.015,353.175 72.015,358.109" id="Fill-477" sketch:type="MSShapeGroup"></path><path d="M72.015,384.907 C72.015,389.841 68.015,393.84 63.082,393.84 C58.148,393.84 54.149,389.841 54.149,384.907 C54.149,379.974 58.148,375.975 63.082,375.975 C68.015,375.975 72.015,379.974 72.015,384.907" id="Fill-478" sketch:type="MSShapeGroup"></path><path d="M72.015,411.706 C72.015,416.639 68.015,420.639 63.082,420.639 C58.148,420.639 54.149,416.639 54.149,411.706 C54.149,406.772 58.148,402.773 63.082,402.773 C68.015,402.773 72.015,406.772 72.015,411.706" id="Fill-479" sketch:type="MSShapeGroup"></path><path d="M72.015,438.504 C72.015,443.438 68.015,447.437 63.082,447.437 C58.148,447.437 54.149,443.438 54.149,438.504 C54.149,433.571 58.148,429.572 63.082,429.572 C68.015,429.572 72.015,433.571 72.015,438.504" id="Fill-480" sketch:type="MSShapeGroup"></path><path d="M72.015,465.303 C72.015,470.236 68.015,474.236 63.082,474.236 C58.148,474.236 54.149,470.236 54.149,465.303 C54.149,460.369 58.148,456.37 63.082,456.37 C68.015,456.37 72.015,460.369 72.015,465.303" id="Fill-481" sketch:type="MSShapeGroup"></path><path d="M72.015,492.101 C72.015,497.035 68.015,501.034 63.082,501.034 C58.148,501.034 54.149,497.035 54.149,492.101 C54.149,487.168 58.148,483.168 63.082,483.168 C68.015,483.168 72.015,487.168 72.015,492.101" id="Fill-482" sketch:type="MSShapeGroup"></path><path d="M72.015,518.9 C72.015,523.833 68.015,527.833 63.082,527.833 C58.148,527.833 54.149,523.833 54.149,518.9 C54.149,513.966 58.148,509.967 63.082,509.967 C68.015,509.967 72.015,513.966 72.015,518.9" id="Fill-483" sketch:type="MSShapeGroup"></path><path d="M72.015,545.698 C72.015,550.632 68.015,554.631 63.082,554.631 C58.148,554.631 54.149,550.632 54.149,545.698 C54.149,540.765 58.148,536.766 63.082,536.766 C68.015,536.766 72.015,540.765 72.015,545.698" id="Fill-484" sketch:type="MSShapeGroup"></path><path d="M98.813,197.318 C98.813,202.251 94.814,206.251 89.88,206.251 C84.947,206.251 80.947,202.251 80.947,197.318 C80.947,192.384 84.947,188.385 89.88,188.385 C94.814,188.385 98.813,192.384 98.813,197.318" id="Fill-485" sketch:type="MSShapeGroup"></path><path d="M98.813,250.915 C98.813,255.848 94.814,259.848 89.88,259.848 C84.947,259.848 80.947,255.848 80.947,250.915 C80.947,245.981 84.947,241.982 89.88,241.982 C94.814,241.982 98.813,245.981 98.813,250.915" id="Fill-486" sketch:type="MSShapeGroup"></path><path d="M98.813,277.713 C98.813,282.647 94.814,286.646 89.88,286.646 C84.947,286.646 80.947,282.647 80.947,277.713 C80.947,272.78 84.947,268.78 89.88,268.78 C94.814,268.78 98.813,272.78 98.813,277.713" id="Fill-487" sketch:type="MSShapeGroup"></path><path d="M98.813,304.512 C98.813,309.445 94.814,313.445 89.88,313.445 C84.947,313.445 80.947,309.445 80.947,304.512 C80.947,299.578 84.947,295.579 89.88,295.579 C94.814,295.579 98.813,299.578 98.813,304.512" id="Fill-488" sketch:type="MSShapeGroup"></path><path d="M98.813,331.31 C98.813,336.244 94.814,340.243 89.88,340.243 C84.947,340.243 80.947,336.244 80.947,331.31 C80.947,326.377 84.947,322.378 89.88,322.378 C94.814,322.378 98.813,326.377 98.813,331.31" id="Fill-489" sketch:type="MSShapeGroup"></path><path d="M98.813,358.109 C98.813,363.042 94.814,367.042 89.88,367.042 C84.947,367.042 80.947,363.042 80.947,358.109 C80.947,353.175 84.947,349.176 89.88,349.176 C94.814,349.176 98.813,353.175 98.813,358.109" id="Fill-490" sketch:type="MSShapeGroup"></path><path d="M98.813,384.907 C98.813,389.841 94.814,393.84 89.88,393.84 C84.947,393.84 80.947,389.841 80.947,384.907 C80.947,379.974 84.947,375.975 89.88,375.975 C94.814,375.975 98.813,379.974 98.813,384.907" id="Fill-491" sketch:type="MSShapeGroup"></path><path d="M98.813,411.706 C98.813,416.639 94.814,420.639 89.88,420.639 C84.947,420.639 80.947,416.639 80.947,411.706 C80.947,406.772 84.947,402.773 89.88,402.773 C94.814,402.773 98.813,406.772 98.813,411.706" id="Fill-492" sketch:type="MSShapeGroup"></path><path d="M98.813,438.504 C98.813,443.438 94.814,447.437 89.88,447.437 C84.947,447.437 80.947,443.438 80.947,438.504 C80.947,433.571 84.947,429.572 89.88,429.572 C94.814,429.572 98.813,433.571 98.813,438.504" id="Fill-493" sketch:type="MSShapeGroup"></path><path d="M98.813,465.303 C98.813,470.236 94.814,474.236 89.88,474.236 C84.947,474.236 80.947,470.236 80.947,465.303 C80.947,460.369 84.947,456.37 89.88,456.37 C94.814,456.37 98.813,460.369 98.813,465.303" id="Fill-494" sketch:type="MSShapeGroup"></path><path d="M98.813,492.101 C98.813,497.035 94.814,501.034 89.88,501.034 C84.947,501.034 80.947,497.035 80.947,492.101 C80.947,487.168 84.947,483.168 89.88,483.168 C94.814,483.168 98.813,487.168 98.813,492.101" id="Fill-495" sketch:type="MSShapeGroup"></path><path d="M98.813,518.9 C98.813,523.833 94.814,527.833 89.88,527.833 C84.947,527.833 80.947,523.833 80.947,518.9 C80.947,513.966 84.947,509.967 89.88,509.967 C94.814,509.967 98.813,513.966 98.813,518.9" id="Fill-496" sketch:type="MSShapeGroup"></path><path d="M98.813,545.698 C98.813,550.632 94.814,554.631 89.88,554.631 C84.947,554.631 80.947,550.632 80.947,545.698 C80.947,540.765 84.947,536.766 89.88,536.766 C94.814,536.766 98.813,540.765 98.813,545.698" id="Fill-497" sketch:type="MSShapeGroup"></path><path d="M125.612,250.915 C125.612,255.848 121.612,259.848 116.679,259.848 C111.745,259.848 107.746,255.848 107.746,250.915 C107.746,245.981 111.745,241.982 116.679,241.982 C121.612,241.982 125.612,245.981 125.612,250.915" id="Fill-498" sketch:type="MSShapeGroup"></path><path d="M125.612,277.713 C125.612,282.647 121.612,286.646 116.679,286.646 C111.745,286.646 107.746,282.647 107.746,277.713 C107.746,272.78 111.745,268.78 116.679,268.78 C121.612,268.78 125.612,272.78 125.612,277.713" id="Fill-499" sketch:type="MSShapeGroup"></path><path d="M125.612,304.512 C125.612,309.445 121.612,313.445 116.679,313.445 C111.745,313.445 107.746,309.445 107.746,304.512 C107.746,299.578 111.745,295.579 116.679,295.579 C121.612,295.579 125.612,299.578 125.612,304.512" id="Fill-500" sketch:type="MSShapeGroup"></path><path d="M125.612,331.31 C125.612,336.244 121.612,340.243 116.679,340.243 C111.745,340.243 107.746,336.244 107.746,331.31 C107.746,326.377 111.745,322.378 116.679,322.378 C121.612,322.378 125.612,326.377 125.612,331.31" id="Fill-501" sketch:type="MSShapeGroup"></path><path d="M125.612,358.109 C125.612,363.042 121.612,367.042 116.679,367.042 C111.745,367.042 107.746,363.042 107.746,358.109 C107.746,353.175 111.745,349.176 116.679,349.176 C121.612,349.176 125.612,353.175 125.612,358.109" id="Fill-502" sketch:type="MSShapeGroup"></path><path d="M125.612,384.907 C125.612,389.841 121.612,393.84 116.679,393.84 C111.745,393.84 107.746,389.841 107.746,384.907 C107.746,379.974 111.745,375.975 116.679,375.975 C121.612,375.975 125.612,379.974 125.612,384.907" id="Fill-503" sketch:type="MSShapeGroup"></path><path d="M125.612,411.706 C125.612,416.639 121.612,420.639 116.679,420.639 C111.745,420.639 107.746,416.639 107.746,411.706 C107.746,406.772 111.745,402.773 116.679,402.773 C121.612,402.773 125.612,406.772 125.612,411.706" id="Fill-504" sketch:type="MSShapeGroup"></path><path d="M125.612,438.504 C125.612,443.438 121.612,447.437 116.679,447.437 C111.745,447.437 107.746,443.438 107.746,438.504 C107.746,433.571 111.745,429.572 116.679,429.572 C121.612,429.572 125.612,433.571 125.612,438.504" id="Fill-505" sketch:type="MSShapeGroup"></path><path d="M125.612,465.303 C125.612,470.236 121.612,474.236 116.679,474.236 C111.745,474.236 107.746,470.236 107.746,465.303 C107.746,460.369 111.745,456.37 116.679,456.37 C121.612,456.37 125.612,460.369 125.612,465.303" id="Fill-506" sketch:type="MSShapeGroup"></path><path d="M125.612,492.101 C125.612,497.035 121.612,501.034 116.679,501.034 C111.745,501.034 107.746,497.035 107.746,492.101 C107.746,487.168 111.745,483.168 116.679,483.168 C121.612,483.168 125.612,487.168 125.612,492.101" id="Fill-507" sketch:type="MSShapeGroup"></path><path d="M125.612,518.9 C125.612,523.833 121.612,527.833 116.679,527.833 C111.745,527.833 107.746,523.833 107.746,518.9 C107.746,513.966 111.745,509.967 116.679,509.967 C121.612,509.967 125.612,513.966 125.612,518.9" id="Fill-508" sketch:type="MSShapeGroup"></path><path d="M125.612,545.698 C125.612,550.632 121.612,554.631 116.679,554.631 C111.745,554.631 107.746,550.632 107.746,545.698 C107.746,540.765 111.745,536.766 116.679,536.766 C121.612,536.766 125.612,540.765 125.612,545.698" id="Fill-509" sketch:type="MSShapeGroup"></path><path d="M152.41,116.922 C152.41,121.856 148.41,125.855 143.48,125.855 C138.544,125.855 134.544,121.856 134.544,116.922 C134.544,111.989 138.544,107.989 143.48,107.989 C148.41,107.989 152.41,111.989 152.41,116.922" id="Fill-510" sketch:type="MSShapeGroup"></path><path d="M152.41,143.721 C152.41,148.654 148.41,152.654 143.48,152.654 C138.544,152.654 134.544,148.654 134.544,143.721 C134.544,138.787 138.544,134.788 143.48,134.788 C148.41,134.788 152.41,138.787 152.41,143.721" id="Fill-511" sketch:type="MSShapeGroup"></path><path d="M152.41,170.519 C152.41,175.453 148.41,179.452 143.48,179.452 C138.544,179.452 134.544,175.453 134.544,170.519 C134.544,165.586 138.544,161.586 143.48,161.586 C148.41,161.586 152.41,165.586 152.41,170.519" id="Fill-512" sketch:type="MSShapeGroup"></path><path d="M152.41,197.318 C152.41,202.251 148.41,206.251 143.48,206.251 C138.544,206.251 134.544,202.251 134.544,197.318 C134.544,192.384 138.544,188.385 143.48,188.385 C148.41,188.385 152.41,192.384 152.41,197.318" id="Fill-513" sketch:type="MSShapeGroup"></path><path d="M152.41,224.116 C152.41,229.05 148.41,233.049 143.48,233.049 C138.544,233.049 134.544,229.05 134.544,224.116 C134.544,219.183 138.544,215.184 143.48,215.184 C148.41,215.184 152.41,219.183 152.41,224.116" id="Fill-514" sketch:type="MSShapeGroup"></path><path d="M152.41,250.915 C152.41,255.848 148.41,259.848 143.48,259.848 C138.544,259.848 134.544,255.848 134.544,250.915 C134.544,245.981 138.544,241.982 143.48,241.982 C148.41,241.982 152.41,245.981 152.41,250.915" id="Fill-515" sketch:type="MSShapeGroup"></path><path d="M152.41,277.713 C152.41,282.647 148.41,286.646 143.48,286.646 C138.544,286.646 134.544,282.647 134.544,277.713 C134.544,272.78 138.544,268.78 143.48,268.78 C148.41,268.78 152.41,272.78 152.41,277.713" id="Fill-516" sketch:type="MSShapeGroup"></path><path d="M152.41,304.512 C152.41,309.445 148.41,313.445 143.48,313.445 C138.544,313.445 134.544,309.445 134.544,304.512 C134.544,299.578 138.544,295.579 143.48,295.579 C148.41,295.579 152.41,299.578 152.41,304.512" id="Fill-517" sketch:type="MSShapeGroup"></path><path d="M152.41,384.907 C152.41,389.841 148.41,393.84 143.48,393.84 C138.544,393.84 134.544,389.841 134.544,384.907 C134.544,379.974 138.544,375.975 143.48,375.975 C148.41,375.975 152.41,379.974 152.41,384.907" id="Fill-518" sketch:type="MSShapeGroup"></path><path d="M152.41,411.706 C152.41,416.639 148.41,420.639 143.48,420.639 C138.544,420.639 134.544,416.639 134.544,411.706 C134.544,406.772 138.544,402.773 143.48,402.773 C148.41,402.773 152.41,406.772 152.41,411.706" id="Fill-519" sketch:type="MSShapeGroup"></path><path d="M152.41,438.504 C152.41,443.438 148.41,447.437 143.48,447.437 C138.544,447.437 134.544,443.438 134.544,438.504 C134.544,433.571 138.544,429.572 143.48,429.572 C148.41,429.572 152.41,433.571 152.41,438.504" id="Fill-520" sketch:type="MSShapeGroup"></path><path d="M152.41,465.303 C152.41,470.236 148.41,474.236 143.48,474.236 C138.544,474.236 134.544,470.236 134.544,465.303 C134.544,460.369 138.544,456.37 143.48,456.37 C148.41,456.37 152.41,460.369 152.41,465.303" id="Fill-521" sketch:type="MSShapeGroup"></path><path d="M152.41,492.101 C152.41,497.035 148.41,501.034 143.48,501.034 C138.544,501.034 134.544,497.035 134.544,492.101 C134.544,487.168 138.544,483.168 143.48,483.168 C148.41,483.168 152.41,487.168 152.41,492.101" id="Fill-522" sketch:type="MSShapeGroup"></path><path d="M152.41,518.9 C152.41,523.833 148.41,527.833 143.48,527.833 C138.544,527.833 134.544,523.833 134.544,518.9 C134.544,513.966 138.544,509.967 143.48,509.967 C148.41,509.967 152.41,513.966 152.41,518.9" id="Fill-523" sketch:type="MSShapeGroup"></path><path d="M152.41,545.698 C152.41,550.632 148.41,554.631 143.48,554.631 C138.544,554.631 134.544,550.632 134.544,545.698 C134.544,540.765 138.544,536.766 143.48,536.766 C148.41,536.766 152.41,540.765 152.41,545.698" id="Fill-524" sketch:type="MSShapeGroup"></path><path d="M152.41,572.497 C152.41,577.43 148.41,581.43 143.48,581.43 C138.544,581.43 134.544,577.43 134.544,572.497 C134.544,567.563 138.544,563.564 143.48,563.564 C148.41,563.564 152.41,567.563 152.41,572.497" id="Fill-525" sketch:type="MSShapeGroup"></path><path d="M179.21,116.922 C179.21,121.856 175.21,125.855 170.28,125.855 C165.34,125.855 161.34,121.856 161.34,116.922 C161.34,111.989 165.34,107.989 170.28,107.989 C175.21,107.989 179.21,111.989 179.21,116.922" id="Fill-526" sketch:type="MSShapeGroup"></path><path d="M179.21,143.721 C179.21,148.654 175.21,152.654 170.28,152.654 C165.34,152.654 161.34,148.654 161.34,143.721 C161.34,138.787 165.34,134.788 170.28,134.788 C175.21,134.788 179.21,138.787 179.21,143.721" id="Fill-527" sketch:type="MSShapeGroup"></path><path d="M179.21,170.519 C179.21,175.453 175.21,179.452 170.28,179.452 C165.34,179.452 161.34,175.453 161.34,170.519 C161.34,165.586 165.34,161.586 170.28,161.586 C175.21,161.586 179.21,165.586 179.21,170.519" id="Fill-528" sketch:type="MSShapeGroup"></path><path d="M179.21,197.318 C179.21,202.251 175.21,206.251 170.28,206.251 C165.34,206.251 161.34,202.251 161.34,197.318 C161.34,192.384 165.34,188.385 170.28,188.385 C175.21,188.385 179.21,192.384 179.21,197.318" id="Fill-529" sketch:type="MSShapeGroup"></path><path d="M179.21,250.915 C179.21,255.848 175.21,259.848 170.28,259.848 C165.34,259.848 161.34,255.848 161.34,250.915 C161.34,245.981 165.34,241.982 170.28,241.982 C175.21,241.982 179.21,245.981 179.21,250.915" id="Fill-530" sketch:type="MSShapeGroup"></path><path d="M179.21,277.713 C179.21,282.647 175.21,286.646 170.28,286.646 C165.34,286.646 161.34,282.647 161.34,277.713 C161.34,272.78 165.34,268.78 170.28,268.78 C175.21,268.78 179.21,272.78 179.21,277.713" id="Fill-531" sketch:type="MSShapeGroup"></path><path d="M179.21,304.512 C179.21,309.445 175.21,313.445 170.28,313.445 C165.34,313.445 161.34,309.445 161.34,304.512 C161.34,299.578 165.34,295.579 170.28,295.579 C175.21,295.579 179.21,299.578 179.21,304.512" id="Fill-532" sketch:type="MSShapeGroup"></path><path d="M179.21,411.706 C179.21,416.639 175.21,420.639 170.28,420.639 C165.34,420.639 161.34,416.639 161.34,411.706 C161.34,406.772 165.34,402.773 170.28,402.773 C175.21,402.773 179.21,406.772 179.21,411.706" id="Fill-533" sketch:type="MSShapeGroup"></path><path d="M179.21,438.504 C179.21,443.438 175.21,447.437 170.28,447.437 C165.34,447.437 161.34,443.438 161.34,438.504 C161.34,433.571 165.34,429.572 170.28,429.572 C175.21,429.572 179.21,433.571 179.21,438.504" id="Fill-534" sketch:type="MSShapeGroup"></path><path d="M179.21,465.303 C179.21,470.236 175.21,474.236 170.28,474.236 C165.34,474.236 161.34,470.236 161.34,465.303 C161.34,460.369 165.34,456.37 170.28,456.37 C175.21,456.37 179.21,460.369 179.21,465.303" id="Fill-535" sketch:type="MSShapeGroup"></path><path d="M179.21,492.101 C179.21,497.035 175.21,501.034 170.28,501.034 C165.34,501.034 161.34,497.035 161.34,492.101 C161.34,487.168 165.34,483.168 170.28,483.168 C175.21,483.168 179.21,487.168 179.21,492.101" id="Fill-536" sketch:type="MSShapeGroup"></path><path d="M179.21,518.9 C179.21,523.833 175.21,527.833 170.28,527.833 C165.34,527.833 161.34,523.833 161.34,518.9 C161.34,513.966 165.34,509.967 170.28,509.967 C175.21,509.967 179.21,513.966 179.21,518.9" id="Fill-537" sketch:type="MSShapeGroup"></path><path d="M179.21,545.698 C179.21,550.632 175.21,554.631 170.28,554.631 C165.34,554.631 161.34,550.632 161.34,545.698 C161.34,540.765 165.34,536.766 170.28,536.766 C175.21,536.766 179.21,540.765 179.21,545.698" id="Fill-538" sketch:type="MSShapeGroup"></path><path d="M179.21,572.497 C179.21,577.43 175.21,581.43 170.28,581.43 C165.34,581.43 161.34,577.43 161.34,572.497 C161.34,567.563 165.34,563.564 170.28,563.564 C175.21,563.564 179.21,567.563 179.21,572.497" id="Fill-539" sketch:type="MSShapeGroup"></path><path d="M179.21,599.295 C179.21,604.229 175.21,608.228 170.28,608.228 C165.34,608.228 161.34,604.229 161.34,599.295 C161.34,594.362 165.34,590.363 170.28,590.363 C175.21,590.363 179.21,594.362 179.21,599.295" id="Fill-540" sketch:type="MSShapeGroup"></path><path d="M179.21,626.094 C179.21,631.027 175.21,635.027 170.28,635.027 C165.34,635.027 161.34,631.027 161.34,626.094 C161.34,621.16 165.34,617.161 170.28,617.161 C175.21,617.161 179.21,621.16 179.21,626.094" id="Fill-541" sketch:type="MSShapeGroup"></path><path d="M179.21,652.892 C179.21,657.826 175.21,661.825 170.28,661.825 C165.34,661.825 161.34,657.826 161.34,652.892 C161.34,647.959 165.34,643.96 170.28,643.96 C175.21,643.96 179.21,647.959 179.21,652.892" id="Fill-542" sketch:type="MSShapeGroup"></path><path d="M179.21,679.691 C179.21,684.624 175.21,688.624 170.28,688.624 C165.34,688.624 161.34,684.624 161.34,679.691 C161.34,674.757 165.34,670.758 170.28,670.758 C175.21,670.758 179.21,674.757 179.21,679.691" id="Fill-543" sketch:type="MSShapeGroup"></path><path d="M179.21,706.489 C179.21,711.423 175.21,715.422 170.28,715.422 C165.34,715.422 161.34,711.423 161.34,706.489 C161.34,701.556 165.34,697.556 170.28,697.556 C175.21,697.556 179.21,701.556 179.21,706.489" id="Fill-544" sketch:type="MSShapeGroup"></path><path d="M206.01,170.519 C206.01,175.453 202.01,179.452 197.07,179.452 C192.14,179.452 188.14,175.453 188.14,170.519 C188.14,165.586 192.14,161.586 197.07,161.586 C202.01,161.586 206.01,165.586 206.01,170.519" id="Fill-545" sketch:type="MSShapeGroup"></path><path d="M206.01,197.318 C206.01,202.251 202.01,206.251 197.07,206.251 C192.14,206.251 188.14,202.251 188.14,197.318 C188.14,192.384 192.14,188.385 197.07,188.385 C202.01,188.385 206.01,192.384 206.01,197.318" id="Fill-546" sketch:type="MSShapeGroup"></path><path d="M206.01,250.915 C206.01,255.848 202.01,259.848 197.07,259.848 C192.14,259.848 188.14,255.848 188.14,250.915 C188.14,245.981 192.14,241.982 197.07,241.982 C202.01,241.982 206.01,245.981 206.01,250.915" id="Fill-547" sketch:type="MSShapeGroup"></path><path d="M206.01,277.713 C206.01,282.647 202.01,286.646 197.07,286.646 C192.14,286.646 188.14,282.647 188.14,277.713 C188.14,272.78 192.14,268.78 197.07,268.78 C202.01,268.78 206.01,272.78 206.01,277.713" id="Fill-548" sketch:type="MSShapeGroup"></path><path d="M206.01,304.512 C206.01,309.445 202.01,313.445 197.07,313.445 C192.14,313.445 188.14,309.445 188.14,304.512 C188.14,299.578 192.14,295.579 197.07,295.579 C202.01,295.579 206.01,299.578 206.01,304.512" id="Fill-549" sketch:type="MSShapeGroup"></path><path d="M206.01,331.31 C206.01,336.244 202.01,340.243 197.07,340.243 C192.14,340.243 188.14,336.244 188.14,331.31 C188.14,326.377 192.14,322.378 197.07,322.378 C202.01,322.378 206.01,326.377 206.01,331.31" id="Fill-550" sketch:type="MSShapeGroup"></path><path d="M206.01,411.706 C206.01,416.639 202.01,420.639 197.07,420.639 C192.14,420.639 188.14,416.639 188.14,411.706 C188.14,406.772 192.14,402.773 197.07,402.773 C202.01,402.773 206.01,406.772 206.01,411.706" id="Fill-551" sketch:type="MSShapeGroup"></path><path d="M206.01,438.504 C206.01,443.438 202.01,447.437 197.07,447.437 C192.14,447.437 188.14,443.438 188.14,438.504 C188.14,433.571 192.14,429.572 197.07,429.572 C202.01,429.572 206.01,433.571 206.01,438.504" id="Fill-552" sketch:type="MSShapeGroup"></path><path d="M206.01,465.303 C206.01,470.236 202.01,474.236 197.07,474.236 C192.14,474.236 188.14,470.236 188.14,465.303 C188.14,460.369 192.14,456.37 197.07,456.37 C202.01,456.37 206.01,460.369 206.01,465.303" id="Fill-553" sketch:type="MSShapeGroup"></path><path d="M206.01,492.101 C206.01,497.035 202.01,501.034 197.07,501.034 C192.14,501.034 188.14,497.035 188.14,492.101 C188.14,487.168 192.14,483.168 197.07,483.168 C202.01,483.168 206.01,487.168 206.01,492.101" id="Fill-554" sketch:type="MSShapeGroup"></path><path d="M206.01,518.9 C206.01,523.833 202.01,527.833 197.07,527.833 C192.14,527.833 188.14,523.833 188.14,518.9 C188.14,513.966 192.14,509.967 197.07,509.967 C202.01,509.967 206.01,513.966 206.01,518.9" id="Fill-555" sketch:type="MSShapeGroup"></path><path d="M206.01,545.698 C206.01,550.632 202.01,554.631 197.07,554.631 C192.14,554.631 188.14,550.632 188.14,545.698 C188.14,540.765 192.14,536.766 197.07,536.766 C202.01,536.766 206.01,540.765 206.01,545.698" id="Fill-556" sketch:type="MSShapeGroup"></path><path d="M206.01,572.497 C206.01,577.43 202.01,581.43 197.07,581.43 C192.14,581.43 188.14,577.43 188.14,572.497 C188.14,567.563 192.14,563.564 197.07,563.564 C202.01,563.564 206.01,567.563 206.01,572.497" id="Fill-557" sketch:type="MSShapeGroup"></path><path d="M206.01,599.295 C206.01,604.229 202.01,608.228 197.07,608.228 C192.14,608.228 188.14,604.229 188.14,599.295 C188.14,594.362 192.14,590.363 197.07,590.363 C202.01,590.363 206.01,594.362 206.01,599.295" id="Fill-558" sketch:type="MSShapeGroup"></path><path d="M206.01,626.094 C206.01,631.027 202.01,635.027 197.07,635.027 C192.14,635.027 188.14,631.027 188.14,626.094 C188.14,621.16 192.14,617.161 197.07,617.161 C202.01,617.161 206.01,621.16 206.01,626.094" id="Fill-559" sketch:type="MSShapeGroup"></path><path d="M206.01,652.892 C206.01,657.826 202.01,661.825 197.07,661.825 C192.14,661.825 188.14,657.826 188.14,652.892 C188.14,647.959 192.14,643.96 197.07,643.96 C202.01,643.96 206.01,647.959 206.01,652.892" id="Fill-560" sketch:type="MSShapeGroup"></path><path d="M206.01,679.691 C206.01,684.624 202.01,688.624 197.07,688.624 C192.14,688.624 188.14,684.624 188.14,679.691 C188.14,674.757 192.14,670.758 197.07,670.758 C202.01,670.758 206.01,674.757 206.01,679.691" id="Fill-561" sketch:type="MSShapeGroup"></path><path d="M206.01,706.489 C206.01,711.423 202.01,715.422 197.07,715.422 C192.14,715.422 188.14,711.423 188.14,706.489 C188.14,701.556 192.14,697.556 197.07,697.556 C202.01,697.556 206.01,701.556 206.01,706.489" id="Fill-562" sketch:type="MSShapeGroup"></path><path d="M206.01,733.288 C206.01,738.221 202.01,742.221 197.07,742.221 C192.14,742.221 188.14,738.221 188.14,733.288 C188.14,728.354 192.14,724.355 197.07,724.355 C202.01,724.355 206.01,728.354 206.01,733.288" id="Fill-563" sketch:type="MSShapeGroup"></path><path d="M206.01,760.086 C206.01,765.02 202.01,769.019 197.07,769.019 C192.14,769.019 188.14,765.02 188.14,760.086 C188.14,755.153 192.14,751.154 197.07,751.154 C202.01,751.154 206.01,755.153 206.01,760.086" id="Fill-564" sketch:type="MSShapeGroup"></path><path d="M232.81,170.519 C232.81,175.453 228.81,179.452 223.87,179.452 C218.94,179.452 214.94,175.453 214.94,170.519 C214.94,165.586 218.94,161.586 223.87,161.586 C228.81,161.586 232.81,165.586 232.81,170.519" id="Fill-565" sketch:type="MSShapeGroup"></path><path d="M232.81,197.318 C232.81,202.251 228.81,206.251 223.87,206.251 C218.94,206.251 214.94,202.251 214.94,197.318 C214.94,192.384 218.94,188.385 223.87,188.385 C228.81,188.385 232.81,192.384 232.81,197.318" id="Fill-566" sketch:type="MSShapeGroup"></path><path d="M232.81,224.116 C232.81,229.05 228.81,233.049 223.87,233.049 C218.94,233.049 214.94,229.05 214.94,224.116 C214.94,219.183 218.94,215.184 223.87,215.184 C228.81,215.184 232.81,219.183 232.81,224.116" id="Fill-567" sketch:type="MSShapeGroup"></path><path d="M232.81,250.915 C232.81,255.848 228.81,259.848 223.87,259.848 C218.94,259.848 214.94,255.848 214.94,250.915 C214.94,245.981 218.94,241.982 223.87,241.982 C228.81,241.982 232.81,245.981 232.81,250.915" id="Fill-568" sketch:type="MSShapeGroup"></path><path d="M232.81,277.713 C232.81,282.647 228.81,286.646 223.87,286.646 C218.94,286.646 214.94,282.647 214.94,277.713 C214.94,272.78 218.94,268.78 223.87,268.78 C228.81,268.78 232.81,272.78 232.81,277.713" id="Fill-569" sketch:type="MSShapeGroup"></path><path d="M232.81,304.512 C232.81,309.445 228.81,313.445 223.87,313.445 C218.94,313.445 214.94,309.445 214.94,304.512 C214.94,299.578 218.94,295.579 223.87,295.579 C228.81,295.579 232.81,299.578 232.81,304.512" id="Fill-570" sketch:type="MSShapeGroup"></path><path d="M232.81,331.31 C232.81,336.244 228.81,340.243 223.87,340.243 C218.94,340.243 214.94,336.244 214.94,331.31 C214.94,326.377 218.94,322.378 223.87,322.378 C228.81,322.378 232.81,326.377 232.81,331.31" id="Fill-571" sketch:type="MSShapeGroup"></path><path d="M232.81,411.706 C232.81,416.639 228.81,420.639 223.87,420.639 C218.94,420.639 214.94,416.639 214.94,411.706 C214.94,406.772 218.94,402.773 223.87,402.773 C228.81,402.773 232.81,406.772 232.81,411.706" id="Fill-572" sketch:type="MSShapeGroup"></path><path d="M232.81,438.504 C232.81,443.438 228.81,447.437 223.87,447.437 C218.94,447.437 214.94,443.438 214.94,438.504 C214.94,433.571 218.94,429.572 223.87,429.572 C228.81,429.572 232.81,433.571 232.81,438.504" id="Fill-573" sketch:type="MSShapeGroup"></path><path d="M232.81,465.303 C232.81,470.236 228.81,474.236 223.87,474.236 C218.94,474.236 214.94,470.236 214.94,465.303 C214.94,460.369 218.94,456.37 223.87,456.37 C228.81,456.37 232.81,460.369 232.81,465.303" id="Fill-574" sketch:type="MSShapeGroup"></path><path d="M232.81,492.101 C232.81,497.035 228.81,501.034 223.87,501.034 C218.94,501.034 214.94,497.035 214.94,492.101 C214.94,487.168 218.94,483.168 223.87,483.168 C228.81,483.168 232.81,487.168 232.81,492.101" id="Fill-575" sketch:type="MSShapeGroup"></path><path d="M232.81,518.9 C232.81,523.833 228.81,527.833 223.87,527.833 C218.94,527.833 214.94,523.833 214.94,518.9 C214.94,513.966 218.94,509.967 223.87,509.967 C228.81,509.967 232.81,513.966 232.81,518.9" id="Fill-576" sketch:type="MSShapeGroup"></path><path d="M232.81,545.698 C232.81,550.632 228.81,554.631 223.87,554.631 C218.94,554.631 214.94,550.632 214.94,545.698 C214.94,540.765 218.94,536.766 223.87,536.766 C228.81,536.766 232.81,540.765 232.81,545.698" id="Fill-577" sketch:type="MSShapeGroup"></path><path d="M232.81,572.497 C232.81,577.43 228.81,581.43 223.87,581.43 C218.94,581.43 214.94,577.43 214.94,572.497 C214.94,567.563 218.94,563.564 223.87,563.564 C228.81,563.564 232.81,567.563 232.81,572.497" id="Fill-578" sketch:type="MSShapeGroup"></path><path d="M232.81,599.295 C232.81,604.229 228.81,608.228 223.87,608.228 C218.94,608.228 214.94,604.229 214.94,599.295 C214.94,594.362 218.94,590.363 223.87,590.363 C228.81,590.363 232.81,594.362 232.81,599.295" id="Fill-579" sketch:type="MSShapeGroup"></path><path d="M232.81,626.094 C232.81,631.027 228.81,635.027 223.87,635.027 C218.94,635.027 214.94,631.027 214.94,626.094 C214.94,621.16 218.94,617.161 223.87,617.161 C228.81,617.161 232.81,621.16 232.81,626.094" id="Fill-580" sketch:type="MSShapeGroup"></path><path d="M232.81,652.892 C232.81,657.826 228.81,661.825 223.87,661.825 C218.94,661.825 214.94,657.826 214.94,652.892 C214.94,647.959 218.94,643.96 223.87,643.96 C228.81,643.96 232.81,647.959 232.81,652.892" id="Fill-581" sketch:type="MSShapeGroup"></path><path d="M232.81,679.691 C232.81,684.624 228.81,688.624 223.87,688.624 C218.94,688.624 214.94,684.624 214.94,679.691 C214.94,674.757 218.94,670.758 223.87,670.758 C228.81,670.758 232.81,674.757 232.81,679.691" id="Fill-582" sketch:type="MSShapeGroup"></path><path d="M232.81,706.489 C232.81,711.423 228.81,715.422 223.87,715.422 C218.94,715.422 214.94,711.423 214.94,706.489 C214.94,701.556 218.94,697.556 223.87,697.556 C228.81,697.556 232.81,701.556 232.81,706.489" id="Fill-583" sketch:type="MSShapeGroup"></path><path d="M232.81,733.288 C232.81,738.221 228.81,742.221 223.87,742.221 C218.94,742.221 214.94,738.221 214.94,733.288 C214.94,728.354 218.94,724.355 223.87,724.355 C228.81,724.355 232.81,728.354 232.81,733.288" id="Fill-584" sketch:type="MSShapeGroup"></path><path d="M232.81,760.086 C232.81,765.02 228.81,769.019 223.87,769.019 C218.94,769.019 214.94,765.02 214.94,760.086 C214.94,755.153 218.94,751.154 223.87,751.154 C228.81,751.154 232.81,755.153 232.81,760.086" id="Fill-585" sketch:type="MSShapeGroup"></path><path d="M232.81,786.885 C232.81,791.818 228.81,795.818 223.87,795.818 C218.94,795.818 214.94,791.818 214.94,786.885 C214.94,781.951 218.94,777.952 223.87,777.952 C228.81,777.952 232.81,781.951 232.81,786.885" id="Fill-586" sketch:type="MSShapeGroup"></path><path d="M232.81,813.683 C232.81,818.617 228.81,822.616 223.87,822.616 C218.94,822.616 214.94,818.617 214.94,813.683 C214.94,808.75 218.94,804.751 223.87,804.751 C228.81,804.751 232.81,808.75 232.81,813.683" id="Fill-587" sketch:type="MSShapeGroup"></path><path d="M259.6,116.922 C259.6,121.856 255.6,125.855 250.67,125.855 C245.74,125.855 241.74,121.856 241.74,116.922 C241.74,111.989 245.74,107.989 250.67,107.989 C255.6,107.989 259.6,111.989 259.6,116.922" id="Fill-588" sketch:type="MSShapeGroup"></path><path d="M259.6,143.721 C259.6,148.654 255.6,152.654 250.67,152.654 C245.74,152.654 241.74,148.654 241.74,143.721 C241.74,138.787 245.74,134.788 250.67,134.788 C255.6,134.788 259.6,138.787 259.6,143.721" id="Fill-589" sketch:type="MSShapeGroup"></path><path d="M259.6,170.519 C259.6,175.453 255.6,179.452 250.67,179.452 C245.74,179.452 241.74,175.453 241.74,170.519 C241.74,165.586 245.74,161.586 250.67,161.586 C255.6,161.586 259.6,165.586 259.6,170.519" id="Fill-590" sketch:type="MSShapeGroup"></path><path d="M259.6,197.318 C259.6,202.251 255.6,206.251 250.67,206.251 C245.74,206.251 241.74,202.251 241.74,197.318 C241.74,192.384 245.74,188.385 250.67,188.385 C255.6,188.385 259.6,192.384 259.6,197.318" id="Fill-591" sketch:type="MSShapeGroup"></path><path d="M259.6,224.116 C259.6,229.05 255.6,233.049 250.67,233.049 C245.74,233.049 241.74,229.05 241.74,224.116 C241.74,219.183 245.74,215.184 250.67,215.184 C255.6,215.184 259.6,219.183 259.6,224.116" id="Fill-592" sketch:type="MSShapeGroup"></path><path d="M259.6,250.915 C259.6,255.848 255.6,259.848 250.67,259.848 C245.74,259.848 241.74,255.848 241.74,250.915 C241.74,245.981 245.74,241.982 250.67,241.982 C255.6,241.982 259.6,245.981 259.6,250.915" id="Fill-593" sketch:type="MSShapeGroup"></path><path d="M259.6,277.713 C259.6,282.647 255.6,286.646 250.67,286.646 C245.74,286.646 241.74,282.647 241.74,277.713 C241.74,272.78 245.74,268.78 250.67,268.78 C255.6,268.78 259.6,272.78 259.6,277.713" id="Fill-594" sketch:type="MSShapeGroup"></path><path d="M259.6,304.512 C259.6,309.445 255.6,313.445 250.67,313.445 C245.74,313.445 241.74,309.445 241.74,304.512 C241.74,299.578 245.74,295.579 250.67,295.579 C255.6,295.579 259.6,299.578 259.6,304.512" id="Fill-595" sketch:type="MSShapeGroup"></path><path d="M259.6,331.31 C259.6,336.244 255.6,340.243 250.67,340.243 C245.74,340.243 241.74,336.244 241.74,331.31 C241.74,326.377 245.74,322.378 250.67,322.378 C255.6,322.378 259.6,326.377 259.6,331.31" id="Fill-596" sketch:type="MSShapeGroup"></path><path d="M259.6,358.109 C259.6,363.042 255.6,367.042 250.67,367.042 C245.74,367.042 241.74,363.042 241.74,358.109 C241.74,353.175 245.74,349.176 250.67,349.176 C255.6,349.176 259.6,353.175 259.6,358.109" id="Fill-597" sketch:type="MSShapeGroup"></path><path d="M259.6,411.706 C259.6,416.639 255.6,420.639 250.67,420.639 C245.74,420.639 241.74,416.639 241.74,411.706 C241.74,406.772 245.74,402.773 250.67,402.773 C255.6,402.773 259.6,406.772 259.6,411.706" id="Fill-598" sketch:type="MSShapeGroup"></path><path d="M259.6,438.504 C259.6,443.438 255.6,447.437 250.67,447.437 C245.74,447.437 241.74,443.438 241.74,438.504 C241.74,433.571 245.74,429.572 250.67,429.572 C255.6,429.572 259.6,433.571 259.6,438.504" id="Fill-599" sketch:type="MSShapeGroup"></path><path d="M259.6,465.303 C259.6,470.236 255.6,474.236 250.67,474.236 C245.74,474.236 241.74,470.236 241.74,465.303 C241.74,460.369 245.74,456.37 250.67,456.37 C255.6,456.37 259.6,460.369 259.6,465.303" id="Fill-600" sketch:type="MSShapeGroup"></path><path d="M259.6,492.101 C259.6,497.035 255.6,501.034 250.67,501.034 C245.74,501.034 241.74,497.035 241.74,492.101 C241.74,487.168 245.74,483.168 250.67,483.168 C255.6,483.168 259.6,487.168 259.6,492.101" id="Fill-601" sketch:type="MSShapeGroup"></path><path d="M259.6,518.9 C259.6,523.833 255.6,527.833 250.67,527.833 C245.74,527.833 241.74,523.833 241.74,518.9 C241.74,513.966 245.74,509.967 250.67,509.967 C255.6,509.967 259.6,513.966 259.6,518.9" id="Fill-602" sketch:type="MSShapeGroup"></path><path d="M259.6,545.698 C259.6,550.632 255.6,554.631 250.67,554.631 C245.74,554.631 241.74,550.632 241.74,545.698 C241.74,540.765 245.74,536.766 250.67,536.766 C255.6,536.766 259.6,540.765 259.6,545.698" id="Fill-603" sketch:type="MSShapeGroup"></path><path d="M259.6,572.497 C259.6,577.43 255.6,581.43 250.67,581.43 C245.74,581.43 241.74,577.43 241.74,572.497 C241.74,567.563 245.74,563.564 250.67,563.564 C255.6,563.564 259.6,567.563 259.6,572.497" id="Fill-604" sketch:type="MSShapeGroup"></path><path d="M259.6,599.295 C259.6,604.229 255.6,608.228 250.67,608.228 C245.74,608.228 241.74,604.229 241.74,599.295 C241.74,594.362 245.74,590.363 250.67,590.363 C255.6,590.363 259.6,594.362 259.6,599.295" id="Fill-605" sketch:type="MSShapeGroup"></path><path d="M259.6,626.094 C259.6,631.027 255.6,635.027 250.67,635.027 C245.74,635.027 241.74,631.027 241.74,626.094 C241.74,621.16 245.74,617.161 250.67,617.161 C255.6,617.161 259.6,621.16 259.6,626.094" id="Fill-606" sketch:type="MSShapeGroup"></path><path d="M259.6,652.892 C259.6,657.826 255.6,661.825 250.67,661.825 C245.74,661.825 241.74,657.826 241.74,652.892 C241.74,647.959 245.74,643.96 250.67,643.96 C255.6,643.96 259.6,647.959 259.6,652.892" id="Fill-607" sketch:type="MSShapeGroup"></path><path d="M259.6,679.691 C259.6,684.624 255.6,688.624 250.67,688.624 C245.74,688.624 241.74,684.624 241.74,679.691 C241.74,674.757 245.74,670.758 250.67,670.758 C255.6,670.758 259.6,674.757 259.6,679.691" id="Fill-608" sketch:type="MSShapeGroup"></path><path d="M259.6,706.489 C259.6,711.423 255.6,715.422 250.67,715.422 C245.74,715.422 241.74,711.423 241.74,706.489 C241.74,701.556 245.74,697.556 250.67,697.556 C255.6,697.556 259.6,701.556 259.6,706.489" id="Fill-609" sketch:type="MSShapeGroup"></path><path d="M259.6,733.288 C259.6,738.221 255.6,742.221 250.67,742.221 C245.74,742.221 241.74,738.221 241.74,733.288 C241.74,728.354 245.74,724.355 250.67,724.355 C255.6,724.355 259.6,728.354 259.6,733.288" id="Fill-610" sketch:type="MSShapeGroup"></path><path d="M259.6,760.086 C259.6,765.02 255.6,769.019 250.67,769.019 C245.74,769.019 241.74,765.02 241.74,760.086 C241.74,755.153 245.74,751.154 250.67,751.154 C255.6,751.154 259.6,755.153 259.6,760.086" id="Fill-611" sketch:type="MSShapeGroup"></path><path d="M259.6,786.885 C259.6,791.818 255.6,795.818 250.67,795.818 C245.74,795.818 241.74,791.818 241.74,786.885 C241.74,781.951 245.74,777.952 250.67,777.952 C255.6,777.952 259.6,781.951 259.6,786.885" id="Fill-612" sketch:type="MSShapeGroup"></path><path d="M259.6,813.683 C259.6,818.617 255.6,822.616 250.67,822.616 C245.74,822.616 241.74,818.617 241.74,813.683 C241.74,808.75 245.74,804.751 250.67,804.751 C255.6,804.751 259.6,808.75 259.6,813.683" id="Fill-613" sketch:type="MSShapeGroup"></path><path d="M286.4,116.922 C286.4,121.856 282.4,125.855 277.47,125.855 C272.54,125.855 268.54,121.856 268.54,116.922 C268.54,111.989 272.54,107.989 277.47,107.989 C282.4,107.989 286.4,111.989 286.4,116.922" id="Fill-614" sketch:type="MSShapeGroup"></path><path d="M286.4,143.721 C286.4,148.654 282.4,152.654 277.47,152.654 C272.54,152.654 268.54,148.654 268.54,143.721 C268.54,138.787 272.54,134.788 277.47,134.788 C282.4,134.788 286.4,138.787 286.4,143.721" id="Fill-615" sketch:type="MSShapeGroup"></path><path d="M286.4,170.519 C286.4,175.453 282.4,179.452 277.47,179.452 C272.54,179.452 268.54,175.453 268.54,170.519 C268.54,165.586 272.54,161.586 277.47,161.586 C282.4,161.586 286.4,165.586 286.4,170.519" id="Fill-616" sketch:type="MSShapeGroup"></path><path d="M286.4,197.318 C286.4,202.251 282.4,206.251 277.47,206.251 C272.54,206.251 268.54,202.251 268.54,197.318 C268.54,192.384 272.54,188.385 277.47,188.385 C282.4,188.385 286.4,192.384 286.4,197.318" id="Fill-617" sketch:type="MSShapeGroup"></path><path d="M286.4,224.116 C286.4,229.05 282.4,233.049 277.47,233.049 C272.54,233.049 268.54,229.05 268.54,224.116 C268.54,219.183 272.54,215.184 277.47,215.184 C282.4,215.184 286.4,219.183 286.4,224.116" id="Fill-618" sketch:type="MSShapeGroup"></path><path d="M286.4,250.915 C286.4,255.848 282.4,259.848 277.47,259.848 C272.54,259.848 268.54,255.848 268.54,250.915 C268.54,245.981 272.54,241.982 277.47,241.982 C282.4,241.982 286.4,245.981 286.4,250.915" id="Fill-619" sketch:type="MSShapeGroup"></path><path d="M286.4,277.713 C286.4,282.647 282.4,286.646 277.47,286.646 C272.54,286.646 268.54,282.647 268.54,277.713 C268.54,272.78 272.54,268.78 277.47,268.78 C282.4,268.78 286.4,272.78 286.4,277.713" id="Fill-620" sketch:type="MSShapeGroup"></path><path d="M286.4,358.109 C286.4,363.042 282.4,367.042 277.47,367.042 C272.54,367.042 268.54,363.042 268.54,358.109 C268.54,353.175 272.54,349.176 277.47,349.176 C282.4,349.176 286.4,353.175 286.4,358.109" id="Fill-621" sketch:type="MSShapeGroup"></path><path d="M286.4,411.706 C286.4,416.639 282.4,420.639 277.47,420.639 C272.54,420.639 268.54,416.639 268.54,411.706 C268.54,406.772 272.54,402.773 277.47,402.773 C282.4,402.773 286.4,406.772 286.4,411.706" id="Fill-622" sketch:type="MSShapeGroup"></path><path d="M286.4,438.504 C286.4,443.438 282.4,447.437 277.47,447.437 C272.54,447.437 268.54,443.438 268.54,438.504 C268.54,433.571 272.54,429.572 277.47,429.572 C282.4,429.572 286.4,433.571 286.4,438.504" id="Fill-623" sketch:type="MSShapeGroup"></path><path d="M286.4,465.303 C286.4,470.236 282.4,474.236 277.47,474.236 C272.54,474.236 268.54,470.236 268.54,465.303 C268.54,460.369 272.54,456.37 277.47,456.37 C282.4,456.37 286.4,460.369 286.4,465.303" id="Fill-624" sketch:type="MSShapeGroup"></path><path d="M286.4,492.101 C286.4,497.035 282.4,501.034 277.47,501.034 C272.54,501.034 268.54,497.035 268.54,492.101 C268.54,487.168 272.54,483.168 277.47,483.168 C282.4,483.168 286.4,487.168 286.4,492.101" id="Fill-625" sketch:type="MSShapeGroup"></path><path d="M286.4,518.9 C286.4,523.833 282.4,527.833 277.47,527.833 C272.54,527.833 268.54,523.833 268.54,518.9 C268.54,513.966 272.54,509.967 277.47,509.967 C282.4,509.967 286.4,513.966 286.4,518.9" id="Fill-626" sketch:type="MSShapeGroup"></path><path d="M286.4,545.698 C286.4,550.632 282.4,554.631 277.47,554.631 C272.54,554.631 268.54,550.632 268.54,545.698 C268.54,540.765 272.54,536.766 277.47,536.766 C282.4,536.766 286.4,540.765 286.4,545.698" id="Fill-627" sketch:type="MSShapeGroup"></path><path d="M286.4,572.497 C286.4,577.43 282.4,581.43 277.47,581.43 C272.54,581.43 268.54,577.43 268.54,572.497 C268.54,567.563 272.54,563.564 277.47,563.564 C282.4,563.564 286.4,567.563 286.4,572.497" id="Fill-628" sketch:type="MSShapeGroup"></path><path d="M286.4,599.295 C286.4,604.229 282.4,608.228 277.47,608.228 C272.54,608.228 268.54,604.229 268.54,599.295 C268.54,594.362 272.54,590.363 277.47,590.363 C282.4,590.363 286.4,594.362 286.4,599.295" id="Fill-629" sketch:type="MSShapeGroup"></path><path d="M286.4,626.094 C286.4,631.027 282.4,635.027 277.47,635.027 C272.54,635.027 268.54,631.027 268.54,626.094 C268.54,621.16 272.54,617.161 277.47,617.161 C282.4,617.161 286.4,621.16 286.4,626.094" id="Fill-630" sketch:type="MSShapeGroup"></path><path d="M286.4,652.892 C286.4,657.826 282.4,661.825 277.47,661.825 C272.54,661.825 268.54,657.826 268.54,652.892 C268.54,647.959 272.54,643.96 277.47,643.96 C282.4,643.96 286.4,647.959 286.4,652.892" id="Fill-631" sketch:type="MSShapeGroup"></path><path d="M286.4,679.691 C286.4,684.624 282.4,688.624 277.47,688.624 C272.54,688.624 268.54,684.624 268.54,679.691 C268.54,674.757 272.54,670.758 277.47,670.758 C282.4,670.758 286.4,674.757 286.4,679.691" id="Fill-632" sketch:type="MSShapeGroup"></path><path d="M286.4,706.489 C286.4,711.423 282.4,715.422 277.47,715.422 C272.54,715.422 268.54,711.423 268.54,706.489 C268.54,701.556 272.54,697.556 277.47,697.556 C282.4,697.556 286.4,701.556 286.4,706.489" id="Fill-633" sketch:type="MSShapeGroup"></path><path d="M286.4,733.288 C286.4,738.221 282.4,742.221 277.47,742.221 C272.54,742.221 268.54,738.221 268.54,733.288 C268.54,728.354 272.54,724.355 277.47,724.355 C282.4,724.355 286.4,728.354 286.4,733.288" id="Fill-634" sketch:type="MSShapeGroup"></path><path d="M286.4,760.086 C286.4,765.02 282.4,769.019 277.47,769.019 C272.54,769.019 268.54,765.02 268.54,760.086 C268.54,755.153 272.54,751.154 277.47,751.154 C282.4,751.154 286.4,755.153 286.4,760.086" id="Fill-635" sketch:type="MSShapeGroup"></path><path d="M286.4,786.885 C286.4,791.818 282.4,795.818 277.47,795.818 C272.54,795.818 268.54,791.818 268.54,786.885 C268.54,781.951 272.54,777.952 277.47,777.952 C282.4,777.952 286.4,781.951 286.4,786.885" id="Fill-636" sketch:type="MSShapeGroup"></path><path d="M313.2,116.922 C313.2,121.856 309.2,125.855 304.27,125.855 C299.33,125.855 295.34,121.856 295.34,116.922 C295.34,111.989 299.33,107.989 304.27,107.989 C309.2,107.989 313.2,111.989 313.2,116.922" id="Fill-637" sketch:type="MSShapeGroup"></path><path d="M313.2,143.721 C313.2,148.654 309.2,152.654 304.27,152.654 C299.33,152.654 295.34,148.654 295.34,143.721 C295.34,138.787 299.33,134.788 304.27,134.788 C309.2,134.788 313.2,138.787 313.2,143.721" id="Fill-638" sketch:type="MSShapeGroup"></path><path d="M313.2,170.519 C313.2,175.453 309.2,179.452 304.27,179.452 C299.33,179.452 295.34,175.453 295.34,170.519 C295.34,165.586 299.33,161.586 304.27,161.586 C309.2,161.586 313.2,165.586 313.2,170.519" id="Fill-639" sketch:type="MSShapeGroup"></path><path d="M313.2,197.318 C313.2,202.251 309.2,206.251 304.27,206.251 C299.33,206.251 295.34,202.251 295.34,197.318 C295.34,192.384 299.33,188.385 304.27,188.385 C309.2,188.385 313.2,192.384 313.2,197.318" id="Fill-640" sketch:type="MSShapeGroup"></path><path d="M313.2,224.116 C313.2,229.05 309.2,233.049 304.27,233.049 C299.33,233.049 295.34,229.05 295.34,224.116 C295.34,219.183 299.33,215.184 304.27,215.184 C309.2,215.184 313.2,219.183 313.2,224.116" id="Fill-641" sketch:type="MSShapeGroup"></path><path d="M313.2,250.915 C313.2,255.848 309.2,259.848 304.27,259.848 C299.33,259.848 295.34,255.848 295.34,250.915 C295.34,245.981 299.33,241.982 304.27,241.982 C309.2,241.982 313.2,245.981 313.2,250.915" id="Fill-642" sketch:type="MSShapeGroup"></path><path d="M313.2,277.713 C313.2,282.647 309.2,286.646 304.27,286.646 C299.33,286.646 295.34,282.647 295.34,277.713 C295.34,272.78 299.33,268.78 304.27,268.78 C309.2,268.78 313.2,272.78 313.2,277.713" id="Fill-643" sketch:type="MSShapeGroup"></path><path d="M313.2,331.31 C313.2,336.244 309.2,340.243 304.27,340.243 C299.33,340.243 295.34,336.244 295.34,331.31 C295.34,326.377 299.33,322.378 304.27,322.378 C309.2,322.378 313.2,326.377 313.2,331.31" id="Fill-644" sketch:type="MSShapeGroup"></path><path d="M313.2,358.109 C313.2,363.042 309.2,367.042 304.27,367.042 C299.33,367.042 295.34,363.042 295.34,358.109 C295.34,353.175 299.33,349.176 304.27,349.176 C309.2,349.176 313.2,353.175 313.2,358.109" id="Fill-645" sketch:type="MSShapeGroup"></path><path d="M313.2,411.706 C313.2,416.639 309.2,420.639 304.27,420.639 C299.33,420.639 295.34,416.639 295.34,411.706 C295.34,406.772 299.33,402.773 304.27,402.773 C309.2,402.773 313.2,406.772 313.2,411.706" id="Fill-646" sketch:type="MSShapeGroup"></path><path d="M313.2,438.504 C313.2,443.438 309.2,447.437 304.27,447.437 C299.33,447.437 295.34,443.438 295.34,438.504 C295.34,433.571 299.33,429.572 304.27,429.572 C309.2,429.572 313.2,433.571 313.2,438.504" id="Fill-647" sketch:type="MSShapeGroup"></path><path d="M313.2,465.303 C313.2,470.236 309.2,474.236 304.27,474.236 C299.33,474.236 295.34,470.236 295.34,465.303 C295.34,460.369 299.33,456.37 304.27,456.37 C309.2,456.37 313.2,460.369 313.2,465.303" id="Fill-648" sketch:type="MSShapeGroup"></path><path d="M313.2,492.101 C313.2,497.035 309.2,501.034 304.27,501.034 C299.33,501.034 295.34,497.035 295.34,492.101 C295.34,487.168 299.33,483.168 304.27,483.168 C309.2,483.168 313.2,487.168 313.2,492.101" id="Fill-649" sketch:type="MSShapeGroup"></path><path d="M313.2,518.9 C313.2,523.833 309.2,527.833 304.27,527.833 C299.33,527.833 295.34,523.833 295.34,518.9 C295.34,513.966 299.33,509.967 304.27,509.967 C309.2,509.967 313.2,513.966 313.2,518.9" id="Fill-650" sketch:type="MSShapeGroup"></path><path d="M313.2,545.698 C313.2,550.632 309.2,554.631 304.27,554.631 C299.33,554.631 295.34,550.632 295.34,545.698 C295.34,540.765 299.33,536.766 304.27,536.766 C309.2,536.766 313.2,540.765 313.2,545.698" id="Fill-651" sketch:type="MSShapeGroup"></path><path d="M313.2,572.497 C313.2,577.43 309.2,581.43 304.27,581.43 C299.33,581.43 295.34,577.43 295.34,572.497 C295.34,567.563 299.33,563.564 304.27,563.564 C309.2,563.564 313.2,567.563 313.2,572.497" id="Fill-652" sketch:type="MSShapeGroup"></path><path d="M313.2,599.295 C313.2,604.229 309.2,608.228 304.27,608.228 C299.33,608.228 295.34,604.229 295.34,599.295 C295.34,594.362 299.33,590.363 304.27,590.363 C309.2,590.363 313.2,594.362 313.2,599.295" id="Fill-653" sketch:type="MSShapeGroup"></path><path d="M313.2,626.094 C313.2,631.027 309.2,635.027 304.27,635.027 C299.33,635.027 295.34,631.027 295.34,626.094 C295.34,621.16 299.33,617.161 304.27,617.161 C309.2,617.161 313.2,621.16 313.2,626.094" id="Fill-654" sketch:type="MSShapeGroup"></path><path d="M313.2,652.892 C313.2,657.826 309.2,661.825 304.27,661.825 C299.33,661.825 295.34,657.826 295.34,652.892 C295.34,647.959 299.33,643.96 304.27,643.96 C309.2,643.96 313.2,647.959 313.2,652.892" id="Fill-655" sketch:type="MSShapeGroup"></path><path d="M313.2,679.691 C313.2,684.624 309.2,688.624 304.27,688.624 C299.33,688.624 295.34,684.624 295.34,679.691 C295.34,674.757 299.33,670.758 304.27,670.758 C309.2,670.758 313.2,674.757 313.2,679.691" id="Fill-656" sketch:type="MSShapeGroup"></path><path d="M313.2,706.489 C313.2,711.423 309.2,715.422 304.27,715.422 C299.33,715.422 295.34,711.423 295.34,706.489 C295.34,701.556 299.33,697.556 304.27,697.556 C309.2,697.556 313.2,701.556 313.2,706.489" id="Fill-657" sketch:type="MSShapeGroup"></path><path d="M313.2,733.288 C313.2,738.221 309.2,742.221 304.27,742.221 C299.33,742.221 295.34,738.221 295.34,733.288 C295.34,728.354 299.33,724.355 304.27,724.355 C309.2,724.355 313.2,728.354 313.2,733.288" id="Fill-658" sketch:type="MSShapeGroup"></path><path d="M313.2,760.086 C313.2,765.02 309.2,769.019 304.27,769.019 C299.33,769.019 295.34,765.02 295.34,760.086 C295.34,755.153 299.33,751.154 304.27,751.154 C309.2,751.154 313.2,755.153 313.2,760.086" id="Fill-659" sketch:type="MSShapeGroup"></path><path d="M340,116.922 C340,121.856 336,125.855 331.07,125.855 C326.13,125.855 322.13,121.856 322.13,116.922 C322.13,111.989 326.13,107.989 331.07,107.989 C336,107.989 340,111.989 340,116.922" id="Fill-660" sketch:type="MSShapeGroup"></path><path d="M340,143.721 C340,148.654 336,152.654 331.07,152.654 C326.13,152.654 322.13,148.654 322.13,143.721 C322.13,138.787 326.13,134.788 331.07,134.788 C336,134.788 340,138.787 340,143.721" id="Fill-661" sketch:type="MSShapeGroup"></path><path d="M340,170.519 C340,175.453 336,179.452 331.07,179.452 C326.13,179.452 322.13,175.453 322.13,170.519 C322.13,165.586 326.13,161.586 331.07,161.586 C336,161.586 340,165.586 340,170.519" id="Fill-662" sketch:type="MSShapeGroup"></path><path d="M340,197.318 C340,202.251 336,206.251 331.07,206.251 C326.13,206.251 322.13,202.251 322.13,197.318 C322.13,192.384 326.13,188.385 331.07,188.385 C336,188.385 340,192.384 340,197.318" id="Fill-663" sketch:type="MSShapeGroup"></path><path d="M340,224.116 C340,229.05 336,233.049 331.07,233.049 C326.13,233.049 322.13,229.05 322.13,224.116 C322.13,219.183 326.13,215.184 331.07,215.184 C336,215.184 340,219.183 340,224.116" id="Fill-664" sketch:type="MSShapeGroup"></path><path d="M340,250.915 C340,255.848 336,259.848 331.07,259.848 C326.13,259.848 322.13,255.848 322.13,250.915 C322.13,245.981 326.13,241.982 331.07,241.982 C336,241.982 340,245.981 340,250.915" id="Fill-665" sketch:type="MSShapeGroup"></path><path d="M340,277.713 C340,282.647 336,286.646 331.07,286.646 C326.13,286.646 322.13,282.647 322.13,277.713 C322.13,272.78 326.13,268.78 331.07,268.78 C336,268.78 340,272.78 340,277.713" id="Fill-666" sketch:type="MSShapeGroup"></path><path d="M340,304.512 C340,309.445 336,313.445 331.07,313.445 C326.13,313.445 322.13,309.445 322.13,304.512 C322.13,299.578 326.13,295.579 331.07,295.579 C336,295.579 340,299.578 340,304.512" id="Fill-667" sketch:type="MSShapeGroup"></path><path d="M340,331.31 C340,336.244 336,340.243 331.07,340.243 C326.13,340.243 322.13,336.244 322.13,331.31 C322.13,326.377 326.13,322.378 331.07,322.378 C336,322.378 340,326.377 340,331.31" id="Fill-668" sketch:type="MSShapeGroup"></path><path d="M340,358.109 C340,363.042 336,367.042 331.07,367.042 C326.13,367.042 322.13,363.042 322.13,358.109 C322.13,353.175 326.13,349.176 331.07,349.176 C336,349.176 340,353.175 340,358.109" id="Fill-669" sketch:type="MSShapeGroup"></path><path d="M340,384.907 C340,389.841 336,393.84 331.07,393.84 C326.13,393.84 322.13,389.841 322.13,384.907 C322.13,379.974 326.13,375.975 331.07,375.975 C336,375.975 340,379.974 340,384.907" id="Fill-670" sketch:type="MSShapeGroup"></path><path d="M340,411.706 C340,416.639 336,420.639 331.07,420.639 C326.13,420.639 322.13,416.639 322.13,411.706 C322.13,406.772 326.13,402.773 331.07,402.773 C336,402.773 340,406.772 340,411.706" id="Fill-671" sketch:type="MSShapeGroup"></path><path d="M340,438.504 C340,443.438 336,447.437 331.07,447.437 C326.13,447.437 322.13,443.438 322.13,438.504 C322.13,433.571 326.13,429.572 331.07,429.572 C336,429.572 340,433.571 340,438.504" id="Fill-672" sketch:type="MSShapeGroup"></path><path d="M340,465.303 C340,470.236 336,474.236 331.07,474.236 C326.13,474.236 322.13,470.236 322.13,465.303 C322.13,460.369 326.13,456.37 331.07,456.37 C336,456.37 340,460.369 340,465.303" id="Fill-673" sketch:type="MSShapeGroup"></path><path d="M340,492.101 C340,497.035 336,501.034 331.07,501.034 C326.13,501.034 322.13,497.035 322.13,492.101 C322.13,487.168 326.13,483.168 331.07,483.168 C336,483.168 340,487.168 340,492.101" id="Fill-674" sketch:type="MSShapeGroup"></path><path d="M340,518.9 C340,523.833 336,527.833 331.07,527.833 C326.13,527.833 322.13,523.833 322.13,518.9 C322.13,513.966 326.13,509.967 331.07,509.967 C336,509.967 340,513.966 340,518.9" id="Fill-675" sketch:type="MSShapeGroup"></path><path d="M340,545.698 C340,550.632 336,554.631 331.07,554.631 C326.13,554.631 322.13,550.632 322.13,545.698 C322.13,540.765 326.13,536.766 331.07,536.766 C336,536.766 340,540.765 340,545.698" id="Fill-676" sketch:type="MSShapeGroup"></path><path d="M340,572.497 C340,577.43 336,581.43 331.07,581.43 C326.13,581.43 322.13,577.43 322.13,572.497 C322.13,567.563 326.13,563.564 331.07,563.564 C336,563.564 340,567.563 340,572.497" id="Fill-677" sketch:type="MSShapeGroup"></path><path d="M340,599.295 C340,604.229 336,608.228 331.07,608.228 C326.13,608.228 322.13,604.229 322.13,599.295 C322.13,594.362 326.13,590.363 331.07,590.363 C336,590.363 340,594.362 340,599.295" id="Fill-678" sketch:type="MSShapeGroup"></path><path d="M340,626.094 C340,631.027 336,635.027 331.07,635.027 C326.13,635.027 322.13,631.027 322.13,626.094 C322.13,621.16 326.13,617.161 331.07,617.161 C336,617.161 340,621.16 340,626.094" id="Fill-679" sketch:type="MSShapeGroup"></path><path d="M340,733.288 C340,738.221 336,742.221 331.07,742.221 C326.13,742.221 322.13,738.221 322.13,733.288 C322.13,728.354 326.13,724.355 331.07,724.355 C336,724.355 340,728.354 340,733.288" id="Fill-680" sketch:type="MSShapeGroup"></path><path d="M366.8,116.922 C366.8,121.856 362.8,125.855 357.87,125.855 C352.93,125.855 348.93,121.856 348.93,116.922 C348.93,111.989 352.93,107.989 357.87,107.989 C362.8,107.989 366.8,111.989 366.8,116.922" id="Fill-681" sketch:type="MSShapeGroup"></path><path d="M366.8,143.721 C366.8,148.654 362.8,152.654 357.87,152.654 C352.93,152.654 348.93,148.654 348.93,143.721 C348.93,138.787 352.93,134.788 357.87,134.788 C362.8,134.788 366.8,138.787 366.8,143.721" id="Fill-682" sketch:type="MSShapeGroup"></path><path d="M366.8,170.519 C366.8,175.453 362.8,179.452 357.87,179.452 C352.93,179.452 348.93,175.453 348.93,170.519 C348.93,165.586 352.93,161.586 357.87,161.586 C362.8,161.586 366.8,165.586 366.8,170.519" id="Fill-683" sketch:type="MSShapeGroup"></path><path d="M366.8,197.318 C366.8,202.251 362.8,206.251 357.87,206.251 C352.93,206.251 348.93,202.251 348.93,197.318 C348.93,192.384 352.93,188.385 357.87,188.385 C362.8,188.385 366.8,192.384 366.8,197.318" id="Fill-684" sketch:type="MSShapeGroup"></path><path d="M366.8,224.116 C366.8,229.05 362.8,233.049 357.87,233.049 C352.93,233.049 348.93,229.05 348.93,224.116 C348.93,219.183 352.93,215.184 357.87,215.184 C362.8,215.184 366.8,219.183 366.8,224.116" id="Fill-685" sketch:type="MSShapeGroup"></path><path d="M366.8,250.915 C366.8,255.848 362.8,259.848 357.87,259.848 C352.93,259.848 348.93,255.848 348.93,250.915 C348.93,245.981 352.93,241.982 357.87,241.982 C362.8,241.982 366.8,245.981 366.8,250.915" id="Fill-686" sketch:type="MSShapeGroup"></path><path d="M366.8,277.713 C366.8,282.647 362.8,286.646 357.87,286.646 C352.93,286.646 348.93,282.647 348.93,277.713 C348.93,272.78 352.93,268.78 357.87,268.78 C362.8,268.78 366.8,272.78 366.8,277.713" id="Fill-687" sketch:type="MSShapeGroup"></path><path d="M366.8,304.512 C366.8,309.445 362.8,313.445 357.87,313.445 C352.93,313.445 348.93,309.445 348.93,304.512 C348.93,299.578 352.93,295.579 357.87,295.579 C362.8,295.579 366.8,299.578 366.8,304.512" id="Fill-688" sketch:type="MSShapeGroup"></path><path d="M366.8,331.31 C366.8,336.244 362.8,340.243 357.87,340.243 C352.93,340.243 348.93,336.244 348.93,331.31 C348.93,326.377 352.93,322.378 357.87,322.378 C362.8,322.378 366.8,326.377 366.8,331.31" id="Fill-689" sketch:type="MSShapeGroup"></path><path d="M366.8,358.109 C366.8,363.042 362.8,367.042 357.87,367.042 C352.93,367.042 348.93,363.042 348.93,358.109 C348.93,353.175 352.93,349.176 357.87,349.176 C362.8,349.176 366.8,353.175 366.8,358.109" id="Fill-690" sketch:type="MSShapeGroup"></path><path d="M366.8,384.907 C366.8,389.841 362.8,393.84 357.87,393.84 C352.93,393.84 348.93,389.841 348.93,384.907 C348.93,379.974 352.93,375.975 357.87,375.975 C362.8,375.975 366.8,379.974 366.8,384.907" id="Fill-691" sketch:type="MSShapeGroup"></path><path d="M366.8,411.706 C366.8,416.639 362.8,420.639 357.87,420.639 C352.93,420.639 348.93,416.639 348.93,411.706 C348.93,406.772 352.93,402.773 357.87,402.773 C362.8,402.773 366.8,406.772 366.8,411.706" id="Fill-692" sketch:type="MSShapeGroup"></path><path d="M366.8,438.504 C366.8,443.438 362.8,447.437 357.87,447.437 C352.93,447.437 348.93,443.438 348.93,438.504 C348.93,433.571 352.93,429.572 357.87,429.572 C362.8,429.572 366.8,433.571 366.8,438.504" id="Fill-693" sketch:type="MSShapeGroup"></path><path d="M366.8,465.303 C366.8,470.236 362.8,474.236 357.87,474.236 C352.93,474.236 348.93,470.236 348.93,465.303 C348.93,460.369 352.93,456.37 357.87,456.37 C362.8,456.37 366.8,460.369 366.8,465.303" id="Fill-694" sketch:type="MSShapeGroup"></path><path d="M366.8,492.101 C366.8,497.035 362.8,501.034 357.87,501.034 C352.93,501.034 348.93,497.035 348.93,492.101 C348.93,487.168 352.93,483.168 357.87,483.168 C362.8,483.168 366.8,487.168 366.8,492.101" id="Fill-695" sketch:type="MSShapeGroup"></path><path d="M366.8,518.9 C366.8,523.833 362.8,527.833 357.87,527.833 C352.93,527.833 348.93,523.833 348.93,518.9 C348.93,513.966 352.93,509.967 357.87,509.967 C362.8,509.967 366.8,513.966 366.8,518.9" id="Fill-696" sketch:type="MSShapeGroup"></path><path d="M366.8,545.698 C366.8,550.632 362.8,554.631 357.87,554.631 C352.93,554.631 348.93,550.632 348.93,545.698 C348.93,540.765 352.93,536.766 357.87,536.766 C362.8,536.766 366.8,540.765 366.8,545.698" id="Fill-697" sketch:type="MSShapeGroup"></path><path d="M366.8,572.497 C366.8,577.43 362.8,581.43 357.87,581.43 C352.93,581.43 348.93,577.43 348.93,572.497 C348.93,567.563 352.93,563.564 357.87,563.564 C362.8,563.564 366.8,567.563 366.8,572.497" id="Fill-698" sketch:type="MSShapeGroup"></path><path d="M366.8,599.295 C366.8,604.229 362.8,608.228 357.87,608.228 C352.93,608.228 348.93,604.229 348.93,599.295 C348.93,594.362 352.93,590.363 357.87,590.363 C362.8,590.363 366.8,594.362 366.8,599.295" id="Fill-699" sketch:type="MSShapeGroup"></path><path d="M366.8,679.691 C366.8,684.624 362.8,688.624 357.87,688.624 C352.93,688.624 348.93,684.624 348.93,679.691 C348.93,674.757 352.93,670.758 357.87,670.758 C362.8,670.758 366.8,674.757 366.8,679.691" id="Fill-700" sketch:type="MSShapeGroup"></path><path d="M366.8,706.489 C366.8,711.423 362.8,715.422 357.87,715.422 C352.93,715.422 348.93,711.423 348.93,706.489 C348.93,701.556 352.93,697.556 357.87,697.556 C362.8,697.556 366.8,701.556 366.8,706.489" id="Fill-701" sketch:type="MSShapeGroup"></path><path d="M366.8,733.288 C366.8,738.221 362.8,742.221 357.87,742.221 C352.93,742.221 348.93,738.221 348.93,733.288 C348.93,728.354 352.93,724.355 357.87,724.355 C362.8,724.355 366.8,728.354 366.8,733.288" id="Fill-702" sketch:type="MSShapeGroup"></path><path d="M393.6,116.922 C393.6,121.856 389.6,125.855 384.66,125.855 C379.73,125.855 375.73,121.856 375.73,116.922 C375.73,111.989 379.73,107.989 384.66,107.989 C389.6,107.989 393.6,111.989 393.6,116.922" id="Fill-703" sketch:type="MSShapeGroup"></path><path d="M393.6,143.721 C393.6,148.654 389.6,152.654 384.66,152.654 C379.73,152.654 375.73,148.654 375.73,143.721 C375.73,138.787 379.73,134.788 384.66,134.788 C389.6,134.788 393.6,138.787 393.6,143.721" id="Fill-704" sketch:type="MSShapeGroup"></path><path d="M393.6,170.519 C393.6,175.453 389.6,179.452 384.66,179.452 C379.73,179.452 375.73,175.453 375.73,170.519 C375.73,165.586 379.73,161.586 384.66,161.586 C389.6,161.586 393.6,165.586 393.6,170.519" id="Fill-705" sketch:type="MSShapeGroup"></path><path d="M393.6,197.318 C393.6,202.251 389.6,206.251 384.66,206.251 C379.73,206.251 375.73,202.251 375.73,197.318 C375.73,192.384 379.73,188.385 384.66,188.385 C389.6,188.385 393.6,192.384 393.6,197.318" id="Fill-706" sketch:type="MSShapeGroup"></path><path d="M393.6,224.116 C393.6,229.05 389.6,233.049 384.66,233.049 C379.73,233.049 375.73,229.05 375.73,224.116 C375.73,219.183 379.73,215.184 384.66,215.184 C389.6,215.184 393.6,219.183 393.6,224.116" id="Fill-707" sketch:type="MSShapeGroup"></path><path d="M393.6,250.915 C393.6,255.848 389.6,259.848 384.66,259.848 C379.73,259.848 375.73,255.848 375.73,250.915 C375.73,245.981 379.73,241.982 384.66,241.982 C389.6,241.982 393.6,245.981 393.6,250.915" id="Fill-708" sketch:type="MSShapeGroup"></path><path d="M393.6,277.713 C393.6,282.647 389.6,286.646 384.66,286.646 C379.73,286.646 375.73,282.647 375.73,277.713 C375.73,272.78 379.73,268.78 384.66,268.78 C389.6,268.78 393.6,272.78 393.6,277.713" id="Fill-709" sketch:type="MSShapeGroup"></path><path d="M393.6,304.512 C393.6,309.445 389.6,313.445 384.66,313.445 C379.73,313.445 375.73,309.445 375.73,304.512 C375.73,299.578 379.73,295.579 384.66,295.579 C389.6,295.579 393.6,299.578 393.6,304.512" id="Fill-710" sketch:type="MSShapeGroup"></path><path d="M393.6,331.31 C393.6,336.244 389.6,340.243 384.66,340.243 C379.73,340.243 375.73,336.244 375.73,331.31 C375.73,326.377 379.73,322.378 384.66,322.378 C389.6,322.378 393.6,326.377 393.6,331.31" id="Fill-711" sketch:type="MSShapeGroup"></path><path d="M393.6,358.109 C393.6,363.042 389.6,367.042 384.66,367.042 C379.73,367.042 375.73,363.042 375.73,358.109 C375.73,353.175 379.73,349.176 384.66,349.176 C389.6,349.176 393.6,353.175 393.6,358.109" id="Fill-712" sketch:type="MSShapeGroup"></path><path d="M393.6,384.907 C393.6,389.841 389.6,393.84 384.66,393.84 C379.73,393.84 375.73,389.841 375.73,384.907 C375.73,379.974 379.73,375.975 384.66,375.975 C389.6,375.975 393.6,379.974 393.6,384.907" id="Fill-713" sketch:type="MSShapeGroup"></path><path d="M393.6,411.706 C393.6,416.639 389.6,420.639 384.66,420.639 C379.73,420.639 375.73,416.639 375.73,411.706 C375.73,406.772 379.73,402.773 384.66,402.773 C389.6,402.773 393.6,406.772 393.6,411.706" id="Fill-714" sketch:type="MSShapeGroup"></path><path d="M393.6,438.504 C393.6,443.438 389.6,447.437 384.66,447.437 C379.73,447.437 375.73,443.438 375.73,438.504 C375.73,433.571 379.73,429.572 384.66,429.572 C389.6,429.572 393.6,433.571 393.6,438.504" id="Fill-715" sketch:type="MSShapeGroup"></path><path d="M393.6,465.303 C393.6,470.236 389.6,474.236 384.66,474.236 C379.73,474.236 375.73,470.236 375.73,465.303 C375.73,460.369 379.73,456.37 384.66,456.37 C389.6,456.37 393.6,460.369 393.6,465.303" id="Fill-716" sketch:type="MSShapeGroup"></path><path d="M393.6,492.101 C393.6,497.035 389.6,501.034 384.66,501.034 C379.73,501.034 375.73,497.035 375.73,492.101 C375.73,487.168 379.73,483.168 384.66,483.168 C389.6,483.168 393.6,487.168 393.6,492.101" id="Fill-717" sketch:type="MSShapeGroup"></path><path d="M420.4,63.325 C420.4,68.259 416.4,72.258 411.46,72.258 C406.53,72.258 402.53,68.259 402.53,63.325 C402.53,58.392 406.53,54.392 411.46,54.392 C416.4,54.392 420.4,58.392 420.4,63.325" id="Fill-718" sketch:type="MSShapeGroup"></path><path d="M420.4,116.922 C420.4,121.856 416.4,125.855 411.46,125.855 C406.53,125.855 402.53,121.856 402.53,116.922 C402.53,111.989 406.53,107.989 411.46,107.989 C416.4,107.989 420.4,111.989 420.4,116.922" id="Fill-719" sketch:type="MSShapeGroup"></path><path d="M420.4,143.721 C420.4,148.654 416.4,152.654 411.46,152.654 C406.53,152.654 402.53,148.654 402.53,143.721 C402.53,138.787 406.53,134.788 411.46,134.788 C416.4,134.788 420.4,138.787 420.4,143.721" id="Fill-720" sketch:type="MSShapeGroup"></path><path d="M420.4,170.519 C420.4,175.453 416.4,179.452 411.46,179.452 C406.53,179.452 402.53,175.453 402.53,170.519 C402.53,165.586 406.53,161.586 411.46,161.586 C416.4,161.586 420.4,165.586 420.4,170.519" id="Fill-721" sketch:type="MSShapeGroup"></path><path d="M420.4,197.318 C420.4,202.251 416.4,206.251 411.46,206.251 C406.53,206.251 402.53,202.251 402.53,197.318 C402.53,192.384 406.53,188.385 411.46,188.385 C416.4,188.385 420.4,192.384 420.4,197.318" id="Fill-722" sketch:type="MSShapeGroup"></path><path d="M420.4,224.116 C420.4,229.05 416.4,233.049 411.46,233.049 C406.53,233.049 402.53,229.05 402.53,224.116 C402.53,219.183 406.53,215.184 411.46,215.184 C416.4,215.184 420.4,219.183 420.4,224.116" id="Fill-723" sketch:type="MSShapeGroup"></path><path d="M420.4,250.915 C420.4,255.848 416.4,259.848 411.46,259.848 C406.53,259.848 402.53,255.848 402.53,250.915 C402.53,245.981 406.53,241.982 411.46,241.982 C416.4,241.982 420.4,245.981 420.4,250.915" id="Fill-724" sketch:type="MSShapeGroup"></path><path d="M420.4,277.713 C420.4,282.647 416.4,286.646 411.46,286.646 C406.53,286.646 402.53,282.647 402.53,277.713 C402.53,272.78 406.53,268.78 411.46,268.78 C416.4,268.78 420.4,272.78 420.4,277.713" id="Fill-725" sketch:type="MSShapeGroup"></path><path d="M420.4,304.512 C420.4,309.445 416.4,313.445 411.46,313.445 C406.53,313.445 402.53,309.445 402.53,304.512 C402.53,299.578 406.53,295.579 411.46,295.579 C416.4,295.579 420.4,299.578 420.4,304.512" id="Fill-726" sketch:type="MSShapeGroup"></path><path d="M420.4,384.907 C420.4,389.841 416.4,393.84 411.46,393.84 C406.53,393.84 402.53,389.841 402.53,384.907 C402.53,379.974 406.53,375.975 411.46,375.975 C416.4,375.975 420.4,379.974 420.4,384.907" id="Fill-727" sketch:type="MSShapeGroup"></path><path d="M420.4,411.706 C420.4,416.639 416.4,420.639 411.46,420.639 C406.53,420.639 402.53,416.639 402.53,411.706 C402.53,406.772 406.53,402.773 411.46,402.773 C416.4,402.773 420.4,406.772 420.4,411.706" id="Fill-728" sketch:type="MSShapeGroup"></path><path d="M420.4,438.504 C420.4,443.438 416.4,447.437 411.46,447.437 C406.53,447.437 402.53,443.438 402.53,438.504 C402.53,433.571 406.53,429.572 411.46,429.572 C416.4,429.572 420.4,433.571 420.4,438.504" id="Fill-729" sketch:type="MSShapeGroup"></path><path d="M420.4,465.303 C420.4,470.236 416.4,474.236 411.46,474.236 C406.53,474.236 402.53,470.236 402.53,465.303 C402.53,460.369 406.53,456.37 411.46,456.37 C416.4,456.37 420.4,460.369 420.4,465.303" id="Fill-730" sketch:type="MSShapeGroup"></path><path d="M420.4,492.101 C420.4,497.035 416.4,501.034 411.46,501.034 C406.53,501.034 402.53,497.035 402.53,492.101 C402.53,487.168 406.53,483.168 411.46,483.168 C416.4,483.168 420.4,487.168 420.4,492.101" id="Fill-731" sketch:type="MSShapeGroup"></path><path d="M447.19,36.527 C447.19,41.46 443.19,45.46 438.26,45.46 C433.33,45.46 429.33,41.46 429.33,36.527 C429.33,31.593 433.33,27.594 438.26,27.594 C443.19,27.594 447.19,31.593 447.19,36.527" id="Fill-732" sketch:type="MSShapeGroup"></path><path d="M447.19,63.325 C447.19,68.259 443.19,72.258 438.26,72.258 C433.33,72.258 429.33,68.259 429.33,63.325 C429.33,58.392 433.33,54.392 438.26,54.392 C443.19,54.392 447.19,58.392 447.19,63.325" id="Fill-733" sketch:type="MSShapeGroup"></path><path d="M447.19,116.922 C447.19,121.856 443.19,125.855 438.26,125.855 C433.33,125.855 429.33,121.856 429.33,116.922 C429.33,111.989 433.33,107.989 438.26,107.989 C443.19,107.989 447.19,111.989 447.19,116.922" id="Fill-734" sketch:type="MSShapeGroup"></path><path d="M447.19,143.721 C447.19,148.654 443.19,152.654 438.26,152.654 C433.33,152.654 429.33,148.654 429.33,143.721 C429.33,138.787 433.33,134.788 438.26,134.788 C443.19,134.788 447.19,138.787 447.19,143.721" id="Fill-735" sketch:type="MSShapeGroup"></path><path d="M447.19,170.519 C447.19,175.453 443.19,179.452 438.26,179.452 C433.33,179.452 429.33,175.453 429.33,170.519 C429.33,165.586 433.33,161.586 438.26,161.586 C443.19,161.586 447.19,165.586 447.19,170.519" id="Fill-736" sketch:type="MSShapeGroup"></path><path d="M447.19,197.318 C447.19,202.251 443.19,206.251 438.26,206.251 C433.33,206.251 429.33,202.251 429.33,197.318 C429.33,192.384 433.33,188.385 438.26,188.385 C443.19,188.385 447.19,192.384 447.19,197.318" id="Fill-737" sketch:type="MSShapeGroup"></path><path d="M447.19,224.116 C447.19,229.05 443.19,233.049 438.26,233.049 C433.33,233.049 429.33,229.05 429.33,224.116 C429.33,219.183 433.33,215.184 438.26,215.184 C443.19,215.184 447.19,219.183 447.19,224.116" id="Fill-738" sketch:type="MSShapeGroup"></path><path d="M447.19,250.915 C447.19,255.848 443.19,259.848 438.26,259.848 C433.33,259.848 429.33,255.848 429.33,250.915 C429.33,245.981 433.33,241.982 438.26,241.982 C443.19,241.982 447.19,245.981 447.19,250.915" id="Fill-739" sketch:type="MSShapeGroup"></path><path d="M447.19,277.713 C447.19,282.647 443.19,286.646 438.26,286.646 C433.33,286.646 429.33,282.647 429.33,277.713 C429.33,272.78 433.33,268.78 438.26,268.78 C443.19,268.78 447.19,272.78 447.19,277.713" id="Fill-740" sketch:type="MSShapeGroup"></path><path d="M447.19,304.512 C447.19,309.445 443.19,313.445 438.26,313.445 C433.33,313.445 429.33,309.445 429.33,304.512 C429.33,299.578 433.33,295.579 438.26,295.579 C443.19,295.579 447.19,299.578 447.19,304.512" id="Fill-741" sketch:type="MSShapeGroup"></path><path d="M447.19,331.31 C447.19,336.244 443.19,340.243 438.26,340.243 C433.33,340.243 429.33,336.244 429.33,331.31 C429.33,326.377 433.33,322.378 438.26,322.378 C443.19,322.378 447.19,326.377 447.19,331.31" id="Fill-742" sketch:type="MSShapeGroup"></path><path d="M447.19,358.109 C447.19,363.042 443.19,367.042 438.26,367.042 C433.33,367.042 429.33,363.042 429.33,358.109 C429.33,353.175 433.33,349.176 438.26,349.176 C443.19,349.176 447.19,353.175 447.19,358.109" id="Fill-743" sketch:type="MSShapeGroup"></path><path d="M447.19,384.907 C447.19,389.841 443.19,393.84 438.26,393.84 C433.33,393.84 429.33,389.841 429.33,384.907 C429.33,379.974 433.33,375.975 438.26,375.975 C443.19,375.975 447.19,379.974 447.19,384.907" id="Fill-744" sketch:type="MSShapeGroup"></path><path d="M447.19,411.706 C447.19,416.639 443.19,420.639 438.26,420.639 C433.33,420.639 429.33,416.639 429.33,411.706 C429.33,406.772 433.33,402.773 438.26,402.773 C443.19,402.773 447.19,406.772 447.19,411.706" id="Fill-745" sketch:type="MSShapeGroup"></path><path d="M447.19,438.504 C447.19,443.438 443.19,447.437 438.26,447.437 C433.33,447.437 429.33,443.438 429.33,438.504 C429.33,433.571 433.33,429.572 438.26,429.572 C443.19,429.572 447.19,433.571 447.19,438.504" id="Fill-746" sketch:type="MSShapeGroup"></path><path d="M447.19,465.303 C447.19,470.236 443.19,474.236 438.26,474.236 C433.33,474.236 429.33,470.236 429.33,465.303 C429.33,460.369 433.33,456.37 438.26,456.37 C443.19,456.37 447.19,460.369 447.19,465.303" id="Fill-747" sketch:type="MSShapeGroup"></path><path d="M473.99,9.729 C473.99,14.662 469.99,18.661 465.06,18.661 C460.13,18.661 456.13,14.662 456.13,9.729 C456.13,4.795 460.13,0.796 465.06,0.796 C469.99,0.796 473.99,4.795 473.99,9.729" id="Fill-748" sketch:type="MSShapeGroup"></path><path d="M473.99,36.527 C473.99,41.46 469.99,45.46 465.06,45.46 C460.13,45.46 456.13,41.46 456.13,36.527 C456.13,31.593 460.13,27.594 465.06,27.594 C469.99,27.594 473.99,31.593 473.99,36.527" id="Fill-749" sketch:type="MSShapeGroup"></path><path d="M473.99,116.922 C473.99,121.856 469.99,125.855 465.06,125.855 C460.13,125.855 456.13,121.856 456.13,116.922 C456.13,111.989 460.13,107.989 465.06,107.989 C469.99,107.989 473.99,111.989 473.99,116.922" id="Fill-750" sketch:type="MSShapeGroup"></path><path d="M473.99,143.721 C473.99,148.654 469.99,152.654 465.06,152.654 C460.13,152.654 456.13,148.654 456.13,143.721 C456.13,138.787 460.13,134.788 465.06,134.788 C469.99,134.788 473.99,138.787 473.99,143.721" id="Fill-751" sketch:type="MSShapeGroup"></path><path d="M473.99,170.519 C473.99,175.453 469.99,179.452 465.06,179.452 C460.13,179.452 456.13,175.453 456.13,170.519 C456.13,165.586 460.13,161.586 465.06,161.586 C469.99,161.586 473.99,165.586 473.99,170.519" id="Fill-752" sketch:type="MSShapeGroup"></path><path d="M473.99,197.318 C473.99,202.251 469.99,206.251 465.06,206.251 C460.13,206.251 456.13,202.251 456.13,197.318 C456.13,192.384 460.13,188.385 465.06,188.385 C469.99,188.385 473.99,192.384 473.99,197.318" id="Fill-753" sketch:type="MSShapeGroup"></path><path d="M473.99,224.116 C473.99,229.05 469.99,233.049 465.06,233.049 C460.13,233.049 456.13,229.05 456.13,224.116 C456.13,219.183 460.13,215.184 465.06,215.184 C469.99,215.184 473.99,219.183 473.99,224.116" id="Fill-754" sketch:type="MSShapeGroup"></path><path d="M473.99,250.915 C473.99,255.848 469.99,259.848 465.06,259.848 C460.13,259.848 456.13,255.848 456.13,250.915 C456.13,245.981 460.13,241.982 465.06,241.982 C469.99,241.982 473.99,245.981 473.99,250.915" id="Fill-755" sketch:type="MSShapeGroup"></path><path d="M473.99,277.713 C473.99,282.647 469.99,286.646 465.06,286.646 C460.13,286.646 456.13,282.647 456.13,277.713 C456.13,272.78 460.13,268.78 465.06,268.78 C469.99,268.78 473.99,272.78 473.99,277.713" id="Fill-756" sketch:type="MSShapeGroup"></path><path d="M473.99,304.512 C473.99,309.445 469.99,313.445 465.06,313.445 C460.13,313.445 456.13,309.445 456.13,304.512 C456.13,299.578 460.13,295.579 465.06,295.579 C469.99,295.579 473.99,299.578 473.99,304.512" id="Fill-757" sketch:type="MSShapeGroup"></path><path d="M473.99,331.31 C473.99,336.244 469.99,340.243 465.06,340.243 C460.13,340.243 456.13,336.244 456.13,331.31 C456.13,326.377 460.13,322.378 465.06,322.378 C469.99,322.378 473.99,326.377 473.99,331.31" id="Fill-758" sketch:type="MSShapeGroup"></path><path d="M473.99,358.109 C473.99,363.042 469.99,367.042 465.06,367.042 C460.13,367.042 456.13,363.042 456.13,358.109 C456.13,353.175 460.13,349.176 465.06,349.176 C469.99,349.176 473.99,353.175 473.99,358.109" id="Fill-759" sketch:type="MSShapeGroup"></path><path d="M473.99,384.907 C473.99,389.841 469.99,393.84 465.06,393.84 C460.13,393.84 456.13,389.841 456.13,384.907 C456.13,379.974 460.13,375.975 465.06,375.975 C469.99,375.975 473.99,379.974 473.99,384.907" id="Fill-760" sketch:type="MSShapeGroup"></path><path d="M473.99,411.706 C473.99,416.639 469.99,420.639 465.06,420.639 C460.13,420.639 456.13,416.639 456.13,411.706 C456.13,406.772 460.13,402.773 465.06,402.773 C469.99,402.773 473.99,406.772 473.99,411.706" id="Fill-761" sketch:type="MSShapeGroup"></path><path d="M473.99,438.504 C473.99,443.438 469.99,447.437 465.06,447.437 C460.13,447.437 456.13,443.438 456.13,438.504 C456.13,433.571 460.13,429.572 465.06,429.572 C469.99,429.572 473.99,433.571 473.99,438.504" id="Fill-762" sketch:type="MSShapeGroup"></path><path d="M500.79,9.729 C500.79,14.662 496.79,18.661 491.86,18.661 C486.92,18.661 482.92,14.662 482.92,9.729 C482.92,4.795 486.92,0.796 491.86,0.796 C496.79,0.796 500.79,4.795 500.79,9.729" id="Fill-763" sketch:type="MSShapeGroup"></path><path d="M500.79,116.922 C500.79,121.856 496.79,125.855 491.86,125.855 C486.92,125.855 482.92,121.856 482.92,116.922 C482.92,111.989 486.92,107.989 491.86,107.989 C496.79,107.989 500.79,111.989 500.79,116.922" id="Fill-764" sketch:type="MSShapeGroup"></path><path d="M500.79,143.721 C500.79,148.654 496.79,152.654 491.86,152.654 C486.92,152.654 482.92,148.654 482.92,143.721 C482.92,138.787 486.92,134.788 491.86,134.788 C496.79,134.788 500.79,138.787 500.79,143.721" id="Fill-765" sketch:type="MSShapeGroup"></path><path d="M500.79,170.519 C500.79,175.453 496.79,179.452 491.86,179.452 C486.92,179.452 482.92,175.453 482.92,170.519 C482.92,165.586 486.92,161.586 491.86,161.586 C496.79,161.586 500.79,165.586 500.79,170.519" id="Fill-766" sketch:type="MSShapeGroup"></path><path d="M500.79,197.318 C500.79,202.251 496.79,206.251 491.86,206.251 C486.92,206.251 482.92,202.251 482.92,197.318 C482.92,192.384 486.92,188.385 491.86,188.385 C496.79,188.385 500.79,192.384 500.79,197.318" id="Fill-767" sketch:type="MSShapeGroup"></path><path d="M500.79,224.116 C500.79,229.05 496.79,233.049 491.86,233.049 C486.92,233.049 482.92,229.05 482.92,224.116 C482.92,219.183 486.92,215.184 491.86,215.184 C496.79,215.184 500.79,219.183 500.79,224.116" id="Fill-768" sketch:type="MSShapeGroup"></path><path d="M500.79,250.915 C500.79,255.848 496.79,259.848 491.86,259.848 C486.92,259.848 482.92,255.848 482.92,250.915 C482.92,245.981 486.92,241.982 491.86,241.982 C496.79,241.982 500.79,245.981 500.79,250.915" id="Fill-769" sketch:type="MSShapeGroup"></path><path d="M500.79,277.713 C500.79,282.647 496.79,286.646 491.86,286.646 C486.92,286.646 482.92,282.647 482.92,277.713 C482.92,272.78 486.92,268.78 491.86,268.78 C496.79,268.78 500.79,272.78 500.79,277.713" id="Fill-770" sketch:type="MSShapeGroup"></path><path d="M500.79,304.512 C500.79,309.445 496.79,313.445 491.86,313.445 C486.92,313.445 482.92,309.445 482.92,304.512 C482.92,299.578 486.92,295.579 491.86,295.579 C496.79,295.579 500.79,299.578 500.79,304.512" id="Fill-771" sketch:type="MSShapeGroup"></path><path d="M500.79,331.31 C500.79,336.244 496.79,340.243 491.86,340.243 C486.92,340.243 482.92,336.244 482.92,331.31 C482.92,326.377 486.92,322.378 491.86,322.378 C496.79,322.378 500.79,326.377 500.79,331.31" id="Fill-772" sketch:type="MSShapeGroup"></path><path d="M500.79,358.109 C500.79,363.042 496.79,367.042 491.86,367.042 C486.92,367.042 482.92,363.042 482.92,358.109 C482.92,353.175 486.92,349.176 491.86,349.176 C496.79,349.176 500.79,353.175 500.79,358.109" id="Fill-773" sketch:type="MSShapeGroup"></path><path d="M500.79,384.907 C500.79,389.841 496.79,393.84 491.86,393.84 C486.92,393.84 482.92,389.841 482.92,384.907 C482.92,379.974 486.92,375.975 491.86,375.975 C496.79,375.975 500.79,379.974 500.79,384.907" id="Fill-774" sketch:type="MSShapeGroup"></path><path d="M500.79,411.706 C500.79,416.639 496.79,420.639 491.86,420.639 C486.92,420.639 482.92,416.639 482.92,411.706 C482.92,406.772 486.92,402.773 491.86,402.773 C496.79,402.773 500.79,406.772 500.79,411.706" id="Fill-775" sketch:type="MSShapeGroup"></path><path d="M500.79,438.504 C500.79,443.438 496.79,447.437 491.86,447.437 C486.92,447.437 482.92,443.438 482.92,438.504 C482.92,433.571 486.92,429.572 491.86,429.572 C496.79,429.572 500.79,433.571 500.79,438.504" id="Fill-776" sketch:type="MSShapeGroup"></path><path d="M527.59,63.325 C527.59,68.259 523.59,72.258 518.66,72.258 C513.72,72.258 509.72,68.259 509.72,63.325 C509.72,58.392 513.72,54.392 518.66,54.392 C523.59,54.392 527.59,58.392 527.59,63.325" id="Fill-777" sketch:type="MSShapeGroup"></path><path d="M527.59,90.124 C527.59,95.057 523.59,99.057 518.66,99.057 C513.72,99.057 509.72,95.057 509.72,90.124 C509.72,85.19 513.72,81.191 518.66,81.191 C523.59,81.191 527.59,85.19 527.59,90.124" id="Fill-778" sketch:type="MSShapeGroup"></path><path d="M527.59,116.922 C527.59,121.856 523.59,125.855 518.66,125.855 C513.72,125.855 509.72,121.856 509.72,116.922 C509.72,111.989 513.72,107.989 518.66,107.989 C523.59,107.989 527.59,111.989 527.59,116.922" id="Fill-779" sketch:type="MSShapeGroup"></path><path d="M527.59,143.721 C527.59,148.654 523.59,152.654 518.66,152.654 C513.72,152.654 509.72,148.654 509.72,143.721 C509.72,138.787 513.72,134.788 518.66,134.788 C523.59,134.788 527.59,138.787 527.59,143.721" id="Fill-780" sketch:type="MSShapeGroup"></path><path d="M527.59,170.519 C527.59,175.453 523.59,179.452 518.66,179.452 C513.72,179.452 509.72,175.453 509.72,170.519 C509.72,165.586 513.72,161.586 518.66,161.586 C523.59,161.586 527.59,165.586 527.59,170.519" id="Fill-781" sketch:type="MSShapeGroup"></path><path d="M527.59,197.318 C527.59,202.251 523.59,206.251 518.66,206.251 C513.72,206.251 509.72,202.251 509.72,197.318 C509.72,192.384 513.72,188.385 518.66,188.385 C523.59,188.385 527.59,192.384 527.59,197.318" id="Fill-782" sketch:type="MSShapeGroup"></path><path d="M527.59,224.116 C527.59,229.05 523.59,233.049 518.66,233.049 C513.72,233.049 509.72,229.05 509.72,224.116 C509.72,219.183 513.72,215.184 518.66,215.184 C523.59,215.184 527.59,219.183 527.59,224.116" id="Fill-783" sketch:type="MSShapeGroup"></path><path d="M527.59,250.915 C527.59,255.848 523.59,259.848 518.66,259.848 C513.72,259.848 509.72,255.848 509.72,250.915 C509.72,245.981 513.72,241.982 518.66,241.982 C523.59,241.982 527.59,245.981 527.59,250.915" id="Fill-784" sketch:type="MSShapeGroup"></path><path d="M527.59,277.713 C527.59,282.647 523.59,286.646 518.66,286.646 C513.72,286.646 509.72,282.647 509.72,277.713 C509.72,272.78 513.72,268.78 518.66,268.78 C523.59,268.78 527.59,272.78 527.59,277.713" id="Fill-785" sketch:type="MSShapeGroup"></path><path d="M527.59,304.512 C527.59,309.445 523.59,313.445 518.66,313.445 C513.72,313.445 509.72,309.445 509.72,304.512 C509.72,299.578 513.72,295.579 518.66,295.579 C523.59,295.579 527.59,299.578 527.59,304.512" id="Fill-786" sketch:type="MSShapeGroup"></path><path d="M527.59,331.31 C527.59,336.244 523.59,340.243 518.66,340.243 C513.72,340.243 509.72,336.244 509.72,331.31 C509.72,326.377 513.72,322.378 518.66,322.378 C523.59,322.378 527.59,326.377 527.59,331.31" id="Fill-787" sketch:type="MSShapeGroup"></path><path d="M527.59,358.109 C527.59,363.042 523.59,367.042 518.66,367.042 C513.72,367.042 509.72,363.042 509.72,358.109 C509.72,353.175 513.72,349.176 518.66,349.176 C523.59,349.176 527.59,353.175 527.59,358.109" id="Fill-788" sketch:type="MSShapeGroup"></path><path d="M527.59,384.907 C527.59,389.841 523.59,393.84 518.66,393.84 C513.72,393.84 509.72,389.841 509.72,384.907 C509.72,379.974 513.72,375.975 518.66,375.975 C523.59,375.975 527.59,379.974 527.59,384.907" id="Fill-789" sketch:type="MSShapeGroup"></path><path d="M527.59,411.706 C527.59,416.639 523.59,420.639 518.66,420.639 C513.72,420.639 509.72,416.639 509.72,411.706 C509.72,406.772 513.72,402.773 518.66,402.773 C523.59,402.773 527.59,406.772 527.59,411.706" id="Fill-790" sketch:type="MSShapeGroup"></path><path d="M527.59,438.504 C527.59,443.438 523.59,447.437 518.66,447.437 C513.72,447.437 509.72,443.438 509.72,438.504 C509.72,433.571 513.72,429.572 518.66,429.572 C523.59,429.572 527.59,433.571 527.59,438.504" id="Fill-791" sketch:type="MSShapeGroup"></path><path d="M527.59,465.303 C527.59,470.236 523.59,474.236 518.66,474.236 C513.72,474.236 509.72,470.236 509.72,465.303 C509.72,460.369 513.72,456.37 518.66,456.37 C523.59,456.37 527.59,460.369 527.59,465.303" id="Fill-792" sketch:type="MSShapeGroup"></path><path d="M554.39,63.325 C554.39,68.259 550.39,72.258 545.45,72.258 C540.52,72.258 536.52,68.259 536.52,63.325 C536.52,58.392 540.52,54.392 545.45,54.392 C550.39,54.392 554.39,58.392 554.39,63.325" id="Fill-793" sketch:type="MSShapeGroup"></path><path d="M554.39,90.124 C554.39,95.057 550.39,99.057 545.45,99.057 C540.52,99.057 536.52,95.057 536.52,90.124 C536.52,85.19 540.52,81.191 545.45,81.191 C550.39,81.191 554.39,85.19 554.39,90.124" id="Fill-794" sketch:type="MSShapeGroup"></path><path d="M554.39,116.922 C554.39,121.856 550.39,125.855 545.45,125.855 C540.52,125.855 536.52,121.856 536.52,116.922 C536.52,111.989 540.52,107.989 545.45,107.989 C550.39,107.989 554.39,111.989 554.39,116.922" id="Fill-795" sketch:type="MSShapeGroup"></path><path d="M554.39,143.721 C554.39,148.654 550.39,152.654 545.45,152.654 C540.52,152.654 536.52,148.654 536.52,143.721 C536.52,138.787 540.52,134.788 545.45,134.788 C550.39,134.788 554.39,138.787 554.39,143.721" id="Fill-796" sketch:type="MSShapeGroup"></path><path d="M554.39,170.519 C554.39,175.453 550.39,179.452 545.45,179.452 C540.52,179.452 536.52,175.453 536.52,170.519 C536.52,165.586 540.52,161.586 545.45,161.586 C550.39,161.586 554.39,165.586 554.39,170.519" id="Fill-797" sketch:type="MSShapeGroup"></path><path d="M554.39,197.318 C554.39,202.251 550.39,206.251 545.45,206.251 C540.52,206.251 536.52,202.251 536.52,197.318 C536.52,192.384 540.52,188.385 545.45,188.385 C550.39,188.385 554.39,192.384 554.39,197.318" id="Fill-798" sketch:type="MSShapeGroup"></path><path d="M554.39,224.116 C554.39,229.05 550.39,233.049 545.45,233.049 C540.52,233.049 536.52,229.05 536.52,224.116 C536.52,219.183 540.52,215.184 545.45,215.184 C550.39,215.184 554.39,219.183 554.39,224.116" id="Fill-799" sketch:type="MSShapeGroup"></path><path d="M554.39,250.915 C554.39,255.848 550.39,259.848 545.45,259.848 C540.52,259.848 536.52,255.848 536.52,250.915 C536.52,245.981 540.52,241.982 545.45,241.982 C550.39,241.982 554.39,245.981 554.39,250.915" id="Fill-800" sketch:type="MSShapeGroup"></path><path d="M554.39,277.713 C554.39,282.647 550.39,286.646 545.45,286.646 C540.52,286.646 536.52,282.647 536.52,277.713 C536.52,272.78 540.52,268.78 545.45,268.78 C550.39,268.78 554.39,272.78 554.39,277.713" id="Fill-801" sketch:type="MSShapeGroup"></path><path d="M554.39,304.512 C554.39,309.445 550.39,313.445 545.45,313.445 C540.52,313.445 536.52,309.445 536.52,304.512 C536.52,299.578 540.52,295.579 545.45,295.579 C550.39,295.579 554.39,299.578 554.39,304.512" id="Fill-802" sketch:type="MSShapeGroup"></path><path d="M554.39,331.31 C554.39,336.244 550.39,340.243 545.45,340.243 C540.52,340.243 536.52,336.244 536.52,331.31 C536.52,326.377 540.52,322.378 545.45,322.378 C550.39,322.378 554.39,326.377 554.39,331.31" id="Fill-803" sketch:type="MSShapeGroup"></path><path d="M554.39,358.109 C554.39,363.042 550.39,367.042 545.45,367.042 C540.52,367.042 536.52,363.042 536.52,358.109 C536.52,353.175 540.52,349.176 545.45,349.176 C550.39,349.176 554.39,353.175 554.39,358.109" id="Fill-804" sketch:type="MSShapeGroup"></path><path d="M554.39,384.907 C554.39,389.841 550.39,393.84 545.45,393.84 C540.52,393.84 536.52,389.841 536.52,384.907 C536.52,379.974 540.52,375.975 545.45,375.975 C550.39,375.975 554.39,379.974 554.39,384.907" id="Fill-805" sketch:type="MSShapeGroup"></path><path d="M554.39,411.706 C554.39,416.639 550.39,420.639 545.45,420.639 C540.52,420.639 536.52,416.639 536.52,411.706 C536.52,406.772 540.52,402.773 545.45,402.773 C550.39,402.773 554.39,406.772 554.39,411.706" id="Fill-806" sketch:type="MSShapeGroup"></path><path d="M554.39,438.504 C554.39,443.438 550.39,447.437 545.45,447.437 C540.52,447.437 536.52,443.438 536.52,438.504 C536.52,433.571 540.52,429.572 545.45,429.572 C550.39,429.572 554.39,433.571 554.39,438.504" id="Fill-807" sketch:type="MSShapeGroup"></path><path d="M554.39,465.303 C554.39,470.236 550.39,474.236 545.45,474.236 C540.52,474.236 536.52,470.236 536.52,465.303 C536.52,460.369 540.52,456.37 545.45,456.37 C550.39,456.37 554.39,460.369 554.39,465.303" id="Fill-808" sketch:type="MSShapeGroup"></path><path d="M554.39,492.101 C554.39,497.035 550.39,501.034 545.45,501.034 C540.52,501.034 536.52,497.035 536.52,492.101 C536.52,487.168 540.52,483.168 545.45,483.168 C550.39,483.168 554.39,487.168 554.39,492.101" id="Fill-809" sketch:type="MSShapeGroup"></path><path d="M581.19,63.325 C581.19,68.259 577.19,72.258 572.25,72.258 C567.32,72.258 563.32,68.259 563.32,63.325 C563.32,58.392 567.32,54.392 572.25,54.392 C577.19,54.392 581.19,58.392 581.19,63.325" id="Fill-810" sketch:type="MSShapeGroup"></path><path d="M581.19,90.124 C581.19,95.057 577.19,99.057 572.25,99.057 C567.32,99.057 563.32,95.057 563.32,90.124 C563.32,85.19 567.32,81.191 572.25,81.191 C577.19,81.191 581.19,85.19 581.19,90.124" id="Fill-811" sketch:type="MSShapeGroup"></path><path d="M581.19,116.922 C581.19,121.856 577.19,125.855 572.25,125.855 C567.32,125.855 563.32,121.856 563.32,116.922 C563.32,111.989 567.32,107.989 572.25,107.989 C577.19,107.989 581.19,111.989 581.19,116.922" id="Fill-812" sketch:type="MSShapeGroup"></path><path d="M581.19,143.721 C581.19,148.654 577.19,152.654 572.25,152.654 C567.32,152.654 563.32,148.654 563.32,143.721 C563.32,138.787 567.32,134.788 572.25,134.788 C577.19,134.788 581.19,138.787 581.19,143.721" id="Fill-813" sketch:type="MSShapeGroup"></path><path d="M581.19,170.519 C581.19,175.453 577.19,179.452 572.25,179.452 C567.32,179.452 563.32,175.453 563.32,170.519 C563.32,165.586 567.32,161.586 572.25,161.586 C577.19,161.586 581.19,165.586 581.19,170.519" id="Fill-814" sketch:type="MSShapeGroup"></path><path d="M581.19,197.318 C581.19,202.251 577.19,206.251 572.25,206.251 C567.32,206.251 563.32,202.251 563.32,197.318 C563.32,192.384 567.32,188.385 572.25,188.385 C577.19,188.385 581.19,192.384 581.19,197.318" id="Fill-815" sketch:type="MSShapeGroup"></path><path d="M581.19,224.116 C581.19,229.05 577.19,233.049 572.25,233.049 C567.32,233.049 563.32,229.05 563.32,224.116 C563.32,219.183 567.32,215.184 572.25,215.184 C577.19,215.184 581.19,219.183 581.19,224.116" id="Fill-816" sketch:type="MSShapeGroup"></path><path d="M581.19,250.915 C581.19,255.848 577.19,259.848 572.25,259.848 C567.32,259.848 563.32,255.848 563.32,250.915 C563.32,245.981 567.32,241.982 572.25,241.982 C577.19,241.982 581.19,245.981 581.19,250.915" id="Fill-817" sketch:type="MSShapeGroup"></path><path d="M581.19,277.713 C581.19,282.647 577.19,286.646 572.25,286.646 C567.32,286.646 563.32,282.647 563.32,277.713 C563.32,272.78 567.32,268.78 572.25,268.78 C577.19,268.78 581.19,272.78 581.19,277.713" id="Fill-818" sketch:type="MSShapeGroup"></path><path d="M581.19,304.512 C581.19,309.445 577.19,313.445 572.25,313.445 C567.32,313.445 563.32,309.445 563.32,304.512 C563.32,299.578 567.32,295.579 572.25,295.579 C577.19,295.579 581.19,299.578 581.19,304.512" id="Fill-819" sketch:type="MSShapeGroup"></path><path d="M581.19,331.31 C581.19,336.244 577.19,340.243 572.25,340.243 C567.32,340.243 563.32,336.244 563.32,331.31 C563.32,326.377 567.32,322.378 572.25,322.378 C577.19,322.378 581.19,326.377 581.19,331.31" id="Fill-820" sketch:type="MSShapeGroup"></path><path d="M581.19,358.109 C581.19,363.042 577.19,367.042 572.25,367.042 C567.32,367.042 563.32,363.042 563.32,358.109 C563.32,353.175 567.32,349.176 572.25,349.176 C577.19,349.176 581.19,353.175 581.19,358.109" id="Fill-821" sketch:type="MSShapeGroup"></path><path d="M581.19,384.907 C581.19,389.841 577.19,393.84 572.25,393.84 C567.32,393.84 563.32,389.841 563.32,384.907 C563.32,379.974 567.32,375.975 572.25,375.975 C577.19,375.975 581.19,379.974 581.19,384.907" id="Fill-822" sketch:type="MSShapeGroup"></path><path d="M581.19,411.706 C581.19,416.639 577.19,420.639 572.25,420.639 C567.32,420.639 563.32,416.639 563.32,411.706 C563.32,406.772 567.32,402.773 572.25,402.773 C577.19,402.773 581.19,406.772 581.19,411.706" id="Fill-823" sketch:type="MSShapeGroup"></path><path d="M581.19,438.504 C581.19,443.438 577.19,447.437 572.25,447.437 C567.32,447.437 563.32,443.438 563.32,438.504 C563.32,433.571 567.32,429.572 572.25,429.572 C577.19,429.572 581.19,433.571 581.19,438.504" id="Fill-824" sketch:type="MSShapeGroup"></path><path d="M581.19,465.303 C581.19,470.236 577.19,474.236 572.25,474.236 C567.32,474.236 563.32,470.236 563.32,465.303 C563.32,460.369 567.32,456.37 572.25,456.37 C577.19,456.37 581.19,460.369 581.19,465.303" id="Fill-825" sketch:type="MSShapeGroup"></path><path d="M581.19,492.101 C581.19,497.035 577.19,501.034 572.25,501.034 C567.32,501.034 563.32,497.035 563.32,492.101 C563.32,487.168 567.32,483.168 572.25,483.168 C577.19,483.168 581.19,487.168 581.19,492.101" id="Fill-826" sketch:type="MSShapeGroup"></path><path d="M581.19,518.9 C581.19,523.833 577.19,527.833 572.25,527.833 C567.32,527.833 563.32,523.833 563.32,518.9 C563.32,513.966 567.32,509.967 572.25,509.967 C577.19,509.967 581.19,513.966 581.19,518.9" id="Fill-827" sketch:type="MSShapeGroup"></path><path d="M581.19,545.698 C581.19,550.632 577.19,554.631 572.25,554.631 C567.32,554.631 563.32,550.632 563.32,545.698 C563.32,540.765 567.32,536.766 572.25,536.766 C577.19,536.766 581.19,540.765 581.19,545.698" id="Fill-828" sketch:type="MSShapeGroup"></path><path d="M607.98,9.729 C607.98,14.662 603.99,18.661 599.05,18.661 C594.12,18.661 590.12,14.662 590.12,9.729 C590.12,4.795 594.12,0.796 599.05,0.796 C603.99,0.796 607.98,4.795 607.98,9.729" id="Fill-829" sketch:type="MSShapeGroup"></path><path d="M607.98,36.527 C607.98,41.46 603.99,45.46 599.05,45.46 C594.12,45.46 590.12,41.46 590.12,36.527 C590.12,31.593 594.12,27.594 599.05,27.594 C603.99,27.594 607.98,31.593 607.98,36.527" id="Fill-830" sketch:type="MSShapeGroup"></path><path d="M607.98,63.325 C607.98,68.259 603.99,72.258 599.05,72.258 C594.12,72.258 590.12,68.259 590.12,63.325 C590.12,58.392 594.12,54.392 599.05,54.392 C603.99,54.392 607.98,58.392 607.98,63.325" id="Fill-831" sketch:type="MSShapeGroup"></path><path d="M607.98,90.124 C607.98,95.057 603.99,99.057 599.05,99.057 C594.12,99.057 590.12,95.057 590.12,90.124 C590.12,85.19 594.12,81.191 599.05,81.191 C603.99,81.191 607.98,85.19 607.98,90.124" id="Fill-832" sketch:type="MSShapeGroup"></path><path d="M607.98,116.922 C607.98,121.856 603.99,125.855 599.05,125.855 C594.12,125.855 590.12,121.856 590.12,116.922 C590.12,111.989 594.12,107.989 599.05,107.989 C603.99,107.989 607.98,111.989 607.98,116.922" id="Fill-833" sketch:type="MSShapeGroup"></path><path d="M607.98,143.721 C607.98,148.654 603.99,152.654 599.05,152.654 C594.12,152.654 590.12,148.654 590.12,143.721 C590.12,138.787 594.12,134.788 599.05,134.788 C603.99,134.788 607.98,138.787 607.98,143.721" id="Fill-834" sketch:type="MSShapeGroup"></path><path d="M607.98,170.519 C607.98,175.453 603.99,179.452 599.05,179.452 C594.12,179.452 590.12,175.453 590.12,170.519 C590.12,165.586 594.12,161.586 599.05,161.586 C603.99,161.586 607.98,165.586 607.98,170.519" id="Fill-835" sketch:type="MSShapeGroup"></path><path d="M607.98,197.318 C607.98,202.251 603.99,206.251 599.05,206.251 C594.12,206.251 590.12,202.251 590.12,197.318 C590.12,192.384 594.12,188.385 599.05,188.385 C603.99,188.385 607.98,192.384 607.98,197.318" id="Fill-836" sketch:type="MSShapeGroup"></path><path d="M607.98,224.116 C607.98,229.05 603.99,233.049 599.05,233.049 C594.12,233.049 590.12,229.05 590.12,224.116 C590.12,219.183 594.12,215.184 599.05,215.184 C603.99,215.184 607.98,219.183 607.98,224.116" id="Fill-837" sketch:type="MSShapeGroup"></path><path d="M607.98,250.915 C607.98,255.848 603.99,259.848 599.05,259.848 C594.12,259.848 590.12,255.848 590.12,250.915 C590.12,245.981 594.12,241.982 599.05,241.982 C603.99,241.982 607.98,245.981 607.98,250.915" id="Fill-838" sketch:type="MSShapeGroup"></path><path d="M607.98,277.713 C607.98,282.647 603.99,286.646 599.05,286.646 C594.12,286.646 590.12,282.647 590.12,277.713 C590.12,272.78 594.12,268.78 599.05,268.78 C603.99,268.78 607.98,272.78 607.98,277.713" id="Fill-839" sketch:type="MSShapeGroup"></path><path d="M607.98,304.512 C607.98,309.445 603.99,313.445 599.05,313.445 C594.12,313.445 590.12,309.445 590.12,304.512 C590.12,299.578 594.12,295.579 599.05,295.579 C603.99,295.579 607.98,299.578 607.98,304.512" id="Fill-840" sketch:type="MSShapeGroup"></path><path d="M607.98,331.31 C607.98,336.244 603.99,340.243 599.05,340.243 C594.12,340.243 590.12,336.244 590.12,331.31 C590.12,326.377 594.12,322.378 599.05,322.378 C603.99,322.378 607.98,326.377 607.98,331.31" id="Fill-841" sketch:type="MSShapeGroup"></path><path d="M607.98,358.109 C607.98,363.042 603.99,367.042 599.05,367.042 C594.12,367.042 590.12,363.042 590.12,358.109 C590.12,353.175 594.12,349.176 599.05,349.176 C603.99,349.176 607.98,353.175 607.98,358.109" id="Fill-842" sketch:type="MSShapeGroup"></path><path d="M607.98,384.907 C607.98,389.841 603.99,393.84 599.05,393.84 C594.12,393.84 590.12,389.841 590.12,384.907 C590.12,379.974 594.12,375.975 599.05,375.975 C603.99,375.975 607.98,379.974 607.98,384.907" id="Fill-843" sketch:type="MSShapeGroup"></path><path d="M607.98,411.706 C607.98,416.639 603.99,420.639 599.05,420.639 C594.12,420.639 590.12,416.639 590.12,411.706 C590.12,406.772 594.12,402.773 599.05,402.773 C603.99,402.773 607.98,406.772 607.98,411.706" id="Fill-844" sketch:type="MSShapeGroup"></path><path d="M607.98,438.504 C607.98,443.438 603.99,447.437 599.05,447.437 C594.12,447.437 590.12,443.438 590.12,438.504 C590.12,433.571 594.12,429.572 599.05,429.572 C603.99,429.572 607.98,433.571 607.98,438.504" id="Fill-845" sketch:type="MSShapeGroup"></path><path d="M607.98,465.303 C607.98,470.236 603.99,474.236 599.05,474.236 C594.12,474.236 590.12,470.236 590.12,465.303 C590.12,460.369 594.12,456.37 599.05,456.37 C603.99,456.37 607.98,460.369 607.98,465.303" id="Fill-846" sketch:type="MSShapeGroup"></path><path d="M607.98,492.101 C607.98,497.035 603.99,501.034 599.05,501.034 C594.12,501.034 590.12,497.035 590.12,492.101 C590.12,487.168 594.12,483.168 599.05,483.168 C603.99,483.168 607.98,487.168 607.98,492.101" id="Fill-847" sketch:type="MSShapeGroup"></path><path d="M607.98,545.698 C607.98,550.632 603.99,554.631 599.05,554.631 C594.12,554.631 590.12,550.632 590.12,545.698 C590.12,540.765 594.12,536.766 599.05,536.766 C603.99,536.766 607.98,540.765 607.98,545.698" id="Fill-848" sketch:type="MSShapeGroup"></path><path d="M634.78,9.729 C634.78,14.662 630.78,18.661 625.85,18.661 C620.92,18.661 616.92,14.662 616.92,9.729 C616.92,4.795 620.92,0.796 625.85,0.796 C630.78,0.796 634.78,4.795 634.78,9.729" id="Fill-849" sketch:type="MSShapeGroup"></path><path d="M634.78,36.527 C634.78,41.46 630.78,45.46 625.85,45.46 C620.92,45.46 616.92,41.46 616.92,36.527 C616.92,31.593 620.92,27.594 625.85,27.594 C630.78,27.594 634.78,31.593 634.78,36.527" id="Fill-850" sketch:type="MSShapeGroup"></path><path d="M634.78,63.325 C634.78,68.259 630.78,72.258 625.85,72.258 C620.92,72.258 616.92,68.259 616.92,63.325 C616.92,58.392 620.92,54.392 625.85,54.392 C630.78,54.392 634.78,58.392 634.78,63.325" id="Fill-851" sketch:type="MSShapeGroup"></path><path d="M634.78,90.124 C634.78,95.057 630.78,99.057 625.85,99.057 C620.92,99.057 616.92,95.057 616.92,90.124 C616.92,85.19 620.92,81.191 625.85,81.191 C630.78,81.191 634.78,85.19 634.78,90.124" id="Fill-852" sketch:type="MSShapeGroup"></path><path d="M634.78,116.922 C634.78,121.856 630.78,125.855 625.85,125.855 C620.92,125.855 616.92,121.856 616.92,116.922 C616.92,111.989 620.92,107.989 625.85,107.989 C630.78,107.989 634.78,111.989 634.78,116.922" id="Fill-853" sketch:type="MSShapeGroup"></path><path d="M634.78,143.721 C634.78,148.654 630.78,152.654 625.85,152.654 C620.92,152.654 616.92,148.654 616.92,143.721 C616.92,138.787 620.92,134.788 625.85,134.788 C630.78,134.788 634.78,138.787 634.78,143.721" id="Fill-854" sketch:type="MSShapeGroup"></path><path d="M634.78,170.519 C634.78,175.453 630.78,179.452 625.85,179.452 C620.92,179.452 616.92,175.453 616.92,170.519 C616.92,165.586 620.92,161.586 625.85,161.586 C630.78,161.586 634.78,165.586 634.78,170.519" id="Fill-855" sketch:type="MSShapeGroup"></path><path d="M634.78,197.318 C634.78,202.251 630.78,206.251 625.85,206.251 C620.92,206.251 616.92,202.251 616.92,197.318 C616.92,192.384 620.92,188.385 625.85,188.385 C630.78,188.385 634.78,192.384 634.78,197.318" id="Fill-856" sketch:type="MSShapeGroup"></path><path d="M634.78,224.116 C634.78,229.05 630.78,233.049 625.85,233.049 C620.92,233.049 616.92,229.05 616.92,224.116 C616.92,219.183 620.92,215.184 625.85,215.184 C630.78,215.184 634.78,219.183 634.78,224.116" id="Fill-857" sketch:type="MSShapeGroup"></path><path d="M634.78,250.915 C634.78,255.848 630.78,259.848 625.85,259.848 C620.92,259.848 616.92,255.848 616.92,250.915 C616.92,245.981 620.92,241.982 625.85,241.982 C630.78,241.982 634.78,245.981 634.78,250.915" id="Fill-858" sketch:type="MSShapeGroup"></path><path d="M634.78,277.713 C634.78,282.647 630.78,286.646 625.85,286.646 C620.92,286.646 616.92,282.647 616.92,277.713 C616.92,272.78 620.92,268.78 625.85,268.78 C630.78,268.78 634.78,272.78 634.78,277.713" id="Fill-859" sketch:type="MSShapeGroup"></path><path d="M634.78,304.512 C634.78,309.445 630.78,313.445 625.85,313.445 C620.92,313.445 616.92,309.445 616.92,304.512 C616.92,299.578 620.92,295.579 625.85,295.579 C630.78,295.579 634.78,299.578 634.78,304.512" id="Fill-860" sketch:type="MSShapeGroup"></path><path d="M634.78,331.31 C634.78,336.244 630.78,340.243 625.85,340.243 C620.92,340.243 616.92,336.244 616.92,331.31 C616.92,326.377 620.92,322.378 625.85,322.378 C630.78,322.378 634.78,326.377 634.78,331.31" id="Fill-861" sketch:type="MSShapeGroup"></path><path d="M634.78,358.109 C634.78,363.042 630.78,367.042 625.85,367.042 C620.92,367.042 616.92,363.042 616.92,358.109 C616.92,353.175 620.92,349.176 625.85,349.176 C630.78,349.176 634.78,353.175 634.78,358.109" id="Fill-862" sketch:type="MSShapeGroup"></path><path d="M634.78,384.907 C634.78,389.841 630.78,393.84 625.85,393.84 C620.92,393.84 616.92,389.841 616.92,384.907 C616.92,379.974 620.92,375.975 625.85,375.975 C630.78,375.975 634.78,379.974 634.78,384.907" id="Fill-863" sketch:type="MSShapeGroup"></path><path d="M634.78,411.706 C634.78,416.639 630.78,420.639 625.85,420.639 C620.92,420.639 616.92,416.639 616.92,411.706 C616.92,406.772 620.92,402.773 625.85,402.773 C630.78,402.773 634.78,406.772 634.78,411.706" id="Fill-864" sketch:type="MSShapeGroup"></path><path d="M634.78,438.504 C634.78,443.438 630.78,447.437 625.85,447.437 C620.92,447.437 616.92,443.438 616.92,438.504 C616.92,433.571 620.92,429.572 625.85,429.572 C630.78,429.572 634.78,433.571 634.78,438.504" id="Fill-865" sketch:type="MSShapeGroup"></path><path d="M634.78,465.303 C634.78,470.236 630.78,474.236 625.85,474.236 C620.92,474.236 616.92,470.236 616.92,465.303 C616.92,460.369 620.92,456.37 625.85,456.37 C630.78,456.37 634.78,460.369 634.78,465.303" id="Fill-866" sketch:type="MSShapeGroup"></path><path d="M661.58,9.729 C661.58,14.662 657.58,18.661 652.65,18.661 C647.72,18.661 643.72,14.662 643.72,9.729 C643.72,4.795 647.72,0.796 652.65,0.796 C657.58,0.796 661.58,4.795 661.58,9.729" id="Fill-867" sketch:type="MSShapeGroup"></path><path d="M661.58,36.527 C661.58,41.46 657.58,45.46 652.65,45.46 C647.72,45.46 643.72,41.46 643.72,36.527 C643.72,31.593 647.72,27.594 652.65,27.594 C657.58,27.594 661.58,31.593 661.58,36.527" id="Fill-868" sketch:type="MSShapeGroup"></path><path d="M661.58,63.325 C661.58,68.259 657.58,72.258 652.65,72.258 C647.72,72.258 643.72,68.259 643.72,63.325 C643.72,58.392 647.72,54.392 652.65,54.392 C657.58,54.392 661.58,58.392 661.58,63.325" id="Fill-869" sketch:type="MSShapeGroup"></path><path d="M661.58,90.124 C661.58,95.057 657.58,99.057 652.65,99.057 C647.72,99.057 643.72,95.057 643.72,90.124 C643.72,85.19 647.72,81.191 652.65,81.191 C657.58,81.191 661.58,85.19 661.58,90.124" id="Fill-870" sketch:type="MSShapeGroup"></path><path d="M661.58,116.922 C661.58,121.856 657.58,125.855 652.65,125.855 C647.72,125.855 643.72,121.856 643.72,116.922 C643.72,111.989 647.72,107.989 652.65,107.989 C657.58,107.989 661.58,111.989 661.58,116.922" id="Fill-871" sketch:type="MSShapeGroup"></path><path d="M661.58,143.721 C661.58,148.654 657.58,152.654 652.65,152.654 C647.72,152.654 643.72,148.654 643.72,143.721 C643.72,138.787 647.72,134.788 652.65,134.788 C657.58,134.788 661.58,138.787 661.58,143.721" id="Fill-872" sketch:type="MSShapeGroup"></path><path d="M661.58,170.519 C661.58,175.453 657.58,179.452 652.65,179.452 C647.72,179.452 643.72,175.453 643.72,170.519 C643.72,165.586 647.72,161.586 652.65,161.586 C657.58,161.586 661.58,165.586 661.58,170.519" id="Fill-873" sketch:type="MSShapeGroup"></path><path d="M661.58,197.318 C661.58,202.251 657.58,206.251 652.65,206.251 C647.72,206.251 643.72,202.251 643.72,197.318 C643.72,192.384 647.72,188.385 652.65,188.385 C657.58,188.385 661.58,192.384 661.58,197.318" id="Fill-874" sketch:type="MSShapeGroup"></path><path d="M661.58,224.116 C661.58,229.05 657.58,233.049 652.65,233.049 C647.72,233.049 643.72,229.05 643.72,224.116 C643.72,219.183 647.72,215.184 652.65,215.184 C657.58,215.184 661.58,219.183 661.58,224.116" id="Fill-875" sketch:type="MSShapeGroup"></path><path d="M661.58,250.915 C661.58,255.848 657.58,259.848 652.65,259.848 C647.72,259.848 643.72,255.848 643.72,250.915 C643.72,245.981 647.72,241.982 652.65,241.982 C657.58,241.982 661.58,245.981 661.58,250.915" id="Fill-876" sketch:type="MSShapeGroup"></path><path d="M661.58,277.713 C661.58,282.647 657.58,286.646 652.65,286.646 C647.72,286.646 643.72,282.647 643.72,277.713 C643.72,272.78 647.72,268.78 652.65,268.78 C657.58,268.78 661.58,272.78 661.58,277.713" id="Fill-877" sketch:type="MSShapeGroup"></path><path d="M661.58,304.512 C661.58,309.445 657.58,313.445 652.65,313.445 C647.72,313.445 643.72,309.445 643.72,304.512 C643.72,299.578 647.72,295.579 652.65,295.579 C657.58,295.579 661.58,299.578 661.58,304.512" id="Fill-878" sketch:type="MSShapeGroup"></path><path d="M661.58,331.31 C661.58,336.244 657.58,340.243 652.65,340.243 C647.72,340.243 643.72,336.244 643.72,331.31 C643.72,326.377 647.72,322.378 652.65,322.378 C657.58,322.378 661.58,326.377 661.58,331.31" id="Fill-879" sketch:type="MSShapeGroup"></path><path d="M661.58,358.109 C661.58,363.042 657.58,367.042 652.65,367.042 C647.72,367.042 643.72,363.042 643.72,358.109 C643.72,353.175 647.72,349.176 652.65,349.176 C657.58,349.176 661.58,353.175 661.58,358.109" id="Fill-880" sketch:type="MSShapeGroup"></path><path d="M661.58,384.907 C661.58,389.841 657.58,393.84 652.65,393.84 C647.72,393.84 643.72,389.841 643.72,384.907 C643.72,379.974 647.72,375.975 652.65,375.975 C657.58,375.975 661.58,379.974 661.58,384.907" id="Fill-881" sketch:type="MSShapeGroup"></path><path d="M661.58,411.706 C661.58,416.639 657.58,420.639 652.65,420.639 C647.72,420.639 643.72,416.639 643.72,411.706 C643.72,406.772 647.72,402.773 652.65,402.773 C657.58,402.773 661.58,406.772 661.58,411.706" id="Fill-882" sketch:type="MSShapeGroup"></path><path d="M661.58,438.504 C661.58,443.438 657.58,447.437 652.65,447.437 C647.72,447.437 643.72,443.438 643.72,438.504 C643.72,433.571 647.72,429.572 652.65,429.572 C657.58,429.572 661.58,433.571 661.58,438.504" id="Fill-883" sketch:type="MSShapeGroup"></path><path d="M661.58,465.303 C661.58,470.236 657.58,474.236 652.65,474.236 C647.72,474.236 643.72,470.236 643.72,465.303 C643.72,460.369 647.72,456.37 652.65,456.37 C657.58,456.37 661.58,460.369 661.58,465.303" id="Fill-884" sketch:type="MSShapeGroup"></path><path d="M688.38,9.729 C688.38,14.662 684.38,18.661 679.45,18.661 C674.51,18.661 670.51,14.662 670.51,9.729 C670.51,4.795 674.51,0.796 679.45,0.796 C684.38,0.796 688.38,4.795 688.38,9.729" id="Fill-885" sketch:type="MSShapeGroup"></path><path d="M688.38,36.527 C688.38,41.46 684.38,45.46 679.45,45.46 C674.51,45.46 670.51,41.46 670.51,36.527 C670.51,31.593 674.51,27.594 679.45,27.594 C684.38,27.594 688.38,31.593 688.38,36.527" id="Fill-886" sketch:type="MSShapeGroup"></path><path d="M688.38,63.325 C688.38,68.259 684.38,72.258 679.45,72.258 C674.51,72.258 670.51,68.259 670.51,63.325 C670.51,58.392 674.51,54.392 679.45,54.392 C684.38,54.392 688.38,58.392 688.38,63.325" id="Fill-887" sketch:type="MSShapeGroup"></path><path d="M688.38,90.124 C688.38,95.057 684.38,99.057 679.45,99.057 C674.51,99.057 670.51,95.057 670.51,90.124 C670.51,85.19 674.51,81.191 679.45,81.191 C684.38,81.191 688.38,85.19 688.38,90.124" id="Fill-888" sketch:type="MSShapeGroup"></path><path d="M688.38,116.922 C688.38,121.856 684.38,125.855 679.45,125.855 C674.51,125.855 670.51,121.856 670.51,116.922 C670.51,111.989 674.51,107.989 679.45,107.989 C684.38,107.989 688.38,111.989 688.38,116.922" id="Fill-889" sketch:type="MSShapeGroup"></path><path d="M688.38,143.721 C688.38,148.654 684.38,152.654 679.45,152.654 C674.51,152.654 670.51,148.654 670.51,143.721 C670.51,138.787 674.51,134.788 679.45,134.788 C684.38,134.788 688.38,138.787 688.38,143.721" id="Fill-890" sketch:type="MSShapeGroup"></path><path d="M688.38,170.519 C688.38,175.453 684.38,179.452 679.45,179.452 C674.51,179.452 670.51,175.453 670.51,170.519 C670.51,165.586 674.51,161.586 679.45,161.586 C684.38,161.586 688.38,165.586 688.38,170.519" id="Fill-891" sketch:type="MSShapeGroup"></path><path d="M688.38,197.318 C688.38,202.251 684.38,206.251 679.45,206.251 C674.51,206.251 670.51,202.251 670.51,197.318 C670.51,192.384 674.51,188.385 679.45,188.385 C684.38,188.385 688.38,192.384 688.38,197.318" id="Fill-892" sketch:type="MSShapeGroup"></path><path d="M688.38,224.116 C688.38,229.05 684.38,233.049 679.45,233.049 C674.51,233.049 670.51,229.05 670.51,224.116 C670.51,219.183 674.51,215.184 679.45,215.184 C684.38,215.184 688.38,219.183 688.38,224.116" id="Fill-893" sketch:type="MSShapeGroup"></path><path d="M688.38,250.915 C688.38,255.848 684.38,259.848 679.45,259.848 C674.51,259.848 670.51,255.848 670.51,250.915 C670.51,245.981 674.51,241.982 679.45,241.982 C684.38,241.982 688.38,245.981 688.38,250.915" id="Fill-894" sketch:type="MSShapeGroup"></path><path d="M688.38,277.713 C688.38,282.647 684.38,286.646 679.45,286.646 C674.51,286.646 670.51,282.647 670.51,277.713 C670.51,272.78 674.51,268.78 679.45,268.78 C684.38,268.78 688.38,272.78 688.38,277.713" id="Fill-895" sketch:type="MSShapeGroup"></path><path d="M688.38,304.512 C688.38,309.445 684.38,313.445 679.45,313.445 C674.51,313.445 670.51,309.445 670.51,304.512 C670.51,299.578 674.51,295.579 679.45,295.579 C684.38,295.579 688.38,299.578 688.38,304.512" id="Fill-896" sketch:type="MSShapeGroup"></path><path d="M688.38,331.31 C688.38,336.244 684.38,340.243 679.45,340.243 C674.51,340.243 670.51,336.244 670.51,331.31 C670.51,326.377 674.51,322.378 679.45,322.378 C684.38,322.378 688.38,326.377 688.38,331.31" id="Fill-897" sketch:type="MSShapeGroup"></path><path d="M688.38,358.109 C688.38,363.042 684.38,367.042 679.45,367.042 C674.51,367.042 670.51,363.042 670.51,358.109 C670.51,353.175 674.51,349.176 679.45,349.176 C684.38,349.176 688.38,353.175 688.38,358.109" id="Fill-898" sketch:type="MSShapeGroup"></path><path d="M688.38,384.907 C688.38,389.841 684.38,393.84 679.45,393.84 C674.51,393.84 670.51,389.841 670.51,384.907 C670.51,379.974 674.51,375.975 679.45,375.975 C684.38,375.975 688.38,379.974 688.38,384.907" id="Fill-899" sketch:type="MSShapeGroup"></path><path d="M688.38,411.706 C688.38,416.639 684.38,420.639 679.45,420.639 C674.51,420.639 670.51,416.639 670.51,411.706 C670.51,406.772 674.51,402.773 679.45,402.773 C684.38,402.773 688.38,406.772 688.38,411.706" id="Fill-900" sketch:type="MSShapeGroup"></path><path d="M688.38,438.504 C688.38,443.438 684.38,447.437 679.45,447.437 C674.51,447.437 670.51,443.438 670.51,438.504 C670.51,433.571 674.51,429.572 679.45,429.572 C684.38,429.572 688.38,433.571 688.38,438.504" id="Fill-901" sketch:type="MSShapeGroup"></path><path d="M688.38,465.303 C688.38,470.236 684.38,474.236 679.45,474.236 C674.51,474.236 670.51,470.236 670.51,465.303 C670.51,460.369 674.51,456.37 679.45,456.37 C684.38,456.37 688.38,460.369 688.38,465.303" id="Fill-902" sketch:type="MSShapeGroup"></path><path d="M688.38,492.101 C688.38,497.035 684.38,501.034 679.45,501.034 C674.51,501.034 670.51,497.035 670.51,492.101 C670.51,487.168 674.51,483.168 679.45,483.168 C684.38,483.168 688.38,487.168 688.38,492.101" id="Fill-903" sketch:type="MSShapeGroup"></path><path d="M715.18,9.729 C715.18,14.662 711.18,18.661 706.25,18.661 C701.31,18.661 697.31,14.662 697.31,9.729 C697.31,4.795 701.31,0.796 706.25,0.796 C711.18,0.796 715.18,4.795 715.18,9.729" id="Fill-904" sketch:type="MSShapeGroup"></path><path d="M715.18,36.527 C715.18,41.46 711.18,45.46 706.25,45.46 C701.31,45.46 697.31,41.46 697.31,36.527 C697.31,31.593 701.31,27.594 706.25,27.594 C711.18,27.594 715.18,31.593 715.18,36.527" id="Fill-905" sketch:type="MSShapeGroup"></path><path d="M715.18,63.325 C715.18,68.259 711.18,72.258 706.25,72.258 C701.31,72.258 697.31,68.259 697.31,63.325 C697.31,58.392 701.31,54.392 706.25,54.392 C711.18,54.392 715.18,58.392 715.18,63.325" id="Fill-906" sketch:type="MSShapeGroup"></path><path d="M715.18,90.124 C715.18,95.057 711.18,99.057 706.25,99.057 C701.31,99.057 697.31,95.057 697.31,90.124 C697.31,85.19 701.31,81.191 706.25,81.191 C711.18,81.191 715.18,85.19 715.18,90.124" id="Fill-907" sketch:type="MSShapeGroup"></path><path d="M715.18,116.922 C715.18,121.856 711.18,125.855 706.25,125.855 C701.31,125.855 697.31,121.856 697.31,116.922 C697.31,111.989 701.31,107.989 706.25,107.989 C711.18,107.989 715.18,111.989 715.18,116.922" id="Fill-908" sketch:type="MSShapeGroup"></path><path d="M715.18,143.721 C715.18,148.654 711.18,152.654 706.25,152.654 C701.31,152.654 697.31,148.654 697.31,143.721 C697.31,138.787 701.31,134.788 706.25,134.788 C711.18,134.788 715.18,138.787 715.18,143.721" id="Fill-909" sketch:type="MSShapeGroup"></path><path d="M715.18,170.519 C715.18,175.453 711.18,179.452 706.25,179.452 C701.31,179.452 697.31,175.453 697.31,170.519 C697.31,165.586 701.31,161.586 706.25,161.586 C711.18,161.586 715.18,165.586 715.18,170.519" id="Fill-910" sketch:type="MSShapeGroup"></path><path d="M715.18,197.318 C715.18,202.251 711.18,206.251 706.25,206.251 C701.31,206.251 697.31,202.251 697.31,197.318 C697.31,192.384 701.31,188.385 706.25,188.385 C711.18,188.385 715.18,192.384 715.18,197.318" id="Fill-911" sketch:type="MSShapeGroup"></path><path d="M715.18,224.116 C715.18,229.05 711.18,233.049 706.25,233.049 C701.31,233.049 697.31,229.05 697.31,224.116 C697.31,219.183 701.31,215.184 706.25,215.184 C711.18,215.184 715.18,219.183 715.18,224.116" id="Fill-912" sketch:type="MSShapeGroup"></path><path d="M715.18,250.915 C715.18,255.848 711.18,259.848 706.25,259.848 C701.31,259.848 697.31,255.848 697.31,250.915 C697.31,245.981 701.31,241.982 706.25,241.982 C711.18,241.982 715.18,245.981 715.18,250.915" id="Fill-913" sketch:type="MSShapeGroup"></path><path d="M715.18,277.713 C715.18,282.647 711.18,286.646 706.25,286.646 C701.31,286.646 697.31,282.647 697.31,277.713 C697.31,272.78 701.31,268.78 706.25,268.78 C711.18,268.78 715.18,272.78 715.18,277.713" id="Fill-914" sketch:type="MSShapeGroup"></path><path d="M715.18,304.512 C715.18,309.445 711.18,313.445 706.25,313.445 C701.31,313.445 697.31,309.445 697.31,304.512 C697.31,299.578 701.31,295.579 706.25,295.579 C711.18,295.579 715.18,299.578 715.18,304.512" id="Fill-915" sketch:type="MSShapeGroup"></path><path d="M715.18,331.31 C715.18,336.244 711.18,340.243 706.25,340.243 C701.31,340.243 697.31,336.244 697.31,331.31 C697.31,326.377 701.31,322.378 706.25,322.378 C711.18,322.378 715.18,326.377 715.18,331.31" id="Fill-916" sketch:type="MSShapeGroup"></path><path d="M715.18,358.109 C715.18,363.042 711.18,367.042 706.25,367.042 C701.31,367.042 697.31,363.042 697.31,358.109 C697.31,353.175 701.31,349.176 706.25,349.176 C711.18,349.176 715.18,353.175 715.18,358.109" id="Fill-917" sketch:type="MSShapeGroup"></path><path d="M715.18,384.907 C715.18,389.841 711.18,393.84 706.25,393.84 C701.31,393.84 697.31,389.841 697.31,384.907 C697.31,379.974 701.31,375.975 706.25,375.975 C711.18,375.975 715.18,379.974 715.18,384.907" id="Fill-918" sketch:type="MSShapeGroup"></path><path d="M715.18,411.706 C715.18,416.639 711.18,420.639 706.25,420.639 C701.31,420.639 697.31,416.639 697.31,411.706 C697.31,406.772 701.31,402.773 706.25,402.773 C711.18,402.773 715.18,406.772 715.18,411.706" id="Fill-919" sketch:type="MSShapeGroup"></path><path d="M715.18,438.504 C715.18,443.438 711.18,447.437 706.25,447.437 C701.31,447.437 697.31,443.438 697.31,438.504 C697.31,433.571 701.31,429.572 706.25,429.572 C711.18,429.572 715.18,433.571 715.18,438.504" id="Fill-920" sketch:type="MSShapeGroup"></path><path d="M715.18,465.303 C715.18,470.236 711.18,474.236 706.25,474.236 C701.31,474.236 697.31,470.236 697.31,465.303 C697.31,460.369 701.31,456.37 706.25,456.37 C711.18,456.37 715.18,460.369 715.18,465.303" id="Fill-921" sketch:type="MSShapeGroup"></path><path d="M715.18,492.101 C715.18,497.035 711.18,501.034 706.25,501.034 C701.31,501.034 697.31,497.035 697.31,492.101 C697.31,487.168 701.31,483.168 706.25,483.168 C711.18,483.168 715.18,487.168 715.18,492.101" id="Fill-922" sketch:type="MSShapeGroup"></path><path d="M715.18,518.9 C715.18,523.833 711.18,527.833 706.25,527.833 C701.31,527.833 697.31,523.833 697.31,518.9 C697.31,513.966 701.31,509.967 706.25,509.967 C711.18,509.967 715.18,513.966 715.18,518.9" id="Fill-923" sketch:type="MSShapeGroup"></path><path d="M715.18,545.698 C715.18,550.632 711.18,554.631 706.25,554.631 C701.31,554.631 697.31,550.632 697.31,545.698 C697.31,540.765 701.31,536.766 706.25,536.766 C711.18,536.766 715.18,540.765 715.18,545.698" id="Fill-924" sketch:type="MSShapeGroup"></path><path d="M715.18,572.497 C715.18,577.43 711.18,581.43 706.25,581.43 C701.31,581.43 697.31,577.43 697.31,572.497 C697.31,567.563 701.31,563.564 706.25,563.564 C711.18,563.564 715.18,567.563 715.18,572.497" id="Fill-925" sketch:type="MSShapeGroup"></path><path d="M741.98,9.729 C741.98,14.662 737.98,18.661 733.04,18.661 C728.11,18.661 724.11,14.662 724.11,9.729 C724.11,4.795 728.11,0.796 733.04,0.796 C737.98,0.796 741.98,4.795 741.98,9.729" id="Fill-926" sketch:type="MSShapeGroup"></path><path d="M741.98,36.527 C741.98,41.46 737.98,45.46 733.04,45.46 C728.11,45.46 724.11,41.46 724.11,36.527 C724.11,31.593 728.11,27.594 733.04,27.594 C737.98,27.594 741.98,31.593 741.98,36.527" id="Fill-927" sketch:type="MSShapeGroup"></path><path d="M741.98,63.325 C741.98,68.259 737.98,72.258 733.04,72.258 C728.11,72.258 724.11,68.259 724.11,63.325 C724.11,58.392 728.11,54.392 733.04,54.392 C737.98,54.392 741.98,58.392 741.98,63.325" id="Fill-928" sketch:type="MSShapeGroup"></path><path d="M741.98,90.124 C741.98,95.057 737.98,99.057 733.04,99.057 C728.11,99.057 724.11,95.057 724.11,90.124 C724.11,85.19 728.11,81.191 733.04,81.191 C737.98,81.191 741.98,85.19 741.98,90.124" id="Fill-929" sketch:type="MSShapeGroup"></path><path d="M741.98,116.922 C741.98,121.856 737.98,125.855 733.04,125.855 C728.11,125.855 724.11,121.856 724.11,116.922 C724.11,111.989 728.11,107.989 733.04,107.989 C737.98,107.989 741.98,111.989 741.98,116.922" id="Fill-930" sketch:type="MSShapeGroup"></path><path d="M741.98,143.721 C741.98,148.654 737.98,152.654 733.04,152.654 C728.11,152.654 724.11,148.654 724.11,143.721 C724.11,138.787 728.11,134.788 733.04,134.788 C737.98,134.788 741.98,138.787 741.98,143.721" id="Fill-931" sketch:type="MSShapeGroup"></path><path d="M741.98,170.519 C741.98,175.453 737.98,179.452 733.04,179.452 C728.11,179.452 724.11,175.453 724.11,170.519 C724.11,165.586 728.11,161.586 733.04,161.586 C737.98,161.586 741.98,165.586 741.98,170.519" id="Fill-932" sketch:type="MSShapeGroup"></path><path d="M741.98,197.318 C741.98,202.251 737.98,206.251 733.04,206.251 C728.11,206.251 724.11,202.251 724.11,197.318 C724.11,192.384 728.11,188.385 733.04,188.385 C737.98,188.385 741.98,192.384 741.98,197.318" id="Fill-933" sketch:type="MSShapeGroup"></path><path d="M741.98,224.116 C741.98,229.05 737.98,233.049 733.04,233.049 C728.11,233.049 724.11,229.05 724.11,224.116 C724.11,219.183 728.11,215.184 733.04,215.184 C737.98,215.184 741.98,219.183 741.98,224.116" id="Fill-934" sketch:type="MSShapeGroup"></path><path d="M741.98,250.915 C741.98,255.848 737.98,259.848 733.04,259.848 C728.11,259.848 724.11,255.848 724.11,250.915 C724.11,245.981 728.11,241.982 733.04,241.982 C737.98,241.982 741.98,245.981 741.98,250.915" id="Fill-935" sketch:type="MSShapeGroup"></path><path d="M741.98,277.713 C741.98,282.647 737.98,286.646 733.04,286.646 C728.11,286.646 724.11,282.647 724.11,277.713 C724.11,272.78 728.11,268.78 733.04,268.78 C737.98,268.78 741.98,272.78 741.98,277.713" id="Fill-936" sketch:type="MSShapeGroup"></path><path d="M741.98,304.512 C741.98,309.445 737.98,313.445 733.04,313.445 C728.11,313.445 724.11,309.445 724.11,304.512 C724.11,299.578 728.11,295.579 733.04,295.579 C737.98,295.579 741.98,299.578 741.98,304.512" id="Fill-937" sketch:type="MSShapeGroup"></path><path d="M741.98,331.31 C741.98,336.244 737.98,340.243 733.04,340.243 C728.11,340.243 724.11,336.244 724.11,331.31 C724.11,326.377 728.11,322.378 733.04,322.378 C737.98,322.378 741.98,326.377 741.98,331.31" id="Fill-938" sketch:type="MSShapeGroup"></path><path d="M741.98,358.109 C741.98,363.042 737.98,367.042 733.04,367.042 C728.11,367.042 724.11,363.042 724.11,358.109 C724.11,353.175 728.11,349.176 733.04,349.176 C737.98,349.176 741.98,353.175 741.98,358.109" id="Fill-939" sketch:type="MSShapeGroup"></path><path d="M741.98,384.907 C741.98,389.841 737.98,393.84 733.04,393.84 C728.11,393.84 724.11,389.841 724.11,384.907 C724.11,379.974 728.11,375.975 733.04,375.975 C737.98,375.975 741.98,379.974 741.98,384.907" id="Fill-940" sketch:type="MSShapeGroup"></path><path d="M741.98,438.504 C741.98,443.438 737.98,447.437 733.04,447.437 C728.11,447.437 724.11,443.438 724.11,438.504 C724.11,433.571 728.11,429.572 733.04,429.572 C737.98,429.572 741.98,433.571 741.98,438.504" id="Fill-941" sketch:type="MSShapeGroup"></path><path d="M741.98,465.303 C741.98,470.236 737.98,474.236 733.04,474.236 C728.11,474.236 724.11,470.236 724.11,465.303 C724.11,460.369 728.11,456.37 733.04,456.37 C737.98,456.37 741.98,460.369 741.98,465.303" id="Fill-942" sketch:type="MSShapeGroup"></path><path d="M741.98,492.101 C741.98,497.035 737.98,501.034 733.04,501.034 C728.11,501.034 724.11,497.035 724.11,492.101 C724.11,487.168 728.11,483.168 733.04,483.168 C737.98,483.168 741.98,487.168 741.98,492.101" id="Fill-943" sketch:type="MSShapeGroup"></path><path d="M741.98,518.9 C741.98,523.833 737.98,527.833 733.04,527.833 C728.11,527.833 724.11,523.833 724.11,518.9 C724.11,513.966 728.11,509.967 733.04,509.967 C737.98,509.967 741.98,513.966 741.98,518.9" id="Fill-944" sketch:type="MSShapeGroup"></path><path d="M768.78,9.729 C768.78,14.662 764.78,18.661 759.84,18.661 C754.91,18.661 750.91,14.662 750.91,9.729 C750.91,4.795 754.91,0.796 759.84,0.796 C764.78,0.796 768.78,4.795 768.78,9.729" id="Fill-945" sketch:type="MSShapeGroup"></path><path d="M768.78,36.527 C768.78,41.46 764.78,45.46 759.84,45.46 C754.91,45.46 750.91,41.46 750.91,36.527 C750.91,31.593 754.91,27.594 759.84,27.594 C764.78,27.594 768.78,31.593 768.78,36.527" id="Fill-946" sketch:type="MSShapeGroup"></path><path d="M768.78,63.325 C768.78,68.259 764.78,72.258 759.84,72.258 C754.91,72.258 750.91,68.259 750.91,63.325 C750.91,58.392 754.91,54.392 759.84,54.392 C764.78,54.392 768.78,58.392 768.78,63.325" id="Fill-947" sketch:type="MSShapeGroup"></path><path d="M768.78,90.124 C768.78,95.057 764.78,99.057 759.84,99.057 C754.91,99.057 750.91,95.057 750.91,90.124 C750.91,85.19 754.91,81.191 759.84,81.191 C764.78,81.191 768.78,85.19 768.78,90.124" id="Fill-948" sketch:type="MSShapeGroup"></path><path d="M768.78,116.922 C768.78,121.856 764.78,125.855 759.84,125.855 C754.91,125.855 750.91,121.856 750.91,116.922 C750.91,111.989 754.91,107.989 759.84,107.989 C764.78,107.989 768.78,111.989 768.78,116.922" id="Fill-949" sketch:type="MSShapeGroup"></path><path d="M768.78,143.721 C768.78,148.654 764.78,152.654 759.84,152.654 C754.91,152.654 750.91,148.654 750.91,143.721 C750.91,138.787 754.91,134.788 759.84,134.788 C764.78,134.788 768.78,138.787 768.78,143.721" id="Fill-950" sketch:type="MSShapeGroup"></path><path d="M768.78,170.519 C768.78,175.453 764.78,179.452 759.84,179.452 C754.91,179.452 750.91,175.453 750.91,170.519 C750.91,165.586 754.91,161.586 759.84,161.586 C764.78,161.586 768.78,165.586 768.78,170.519" id="Fill-951" sketch:type="MSShapeGroup"></path><path d="M768.78,197.318 C768.78,202.251 764.78,206.251 759.84,206.251 C754.91,206.251 750.91,202.251 750.91,197.318 C750.91,192.384 754.91,188.385 759.84,188.385 C764.78,188.385 768.78,192.384 768.78,197.318" id="Fill-952" sketch:type="MSShapeGroup"></path><path d="M768.78,224.116 C768.78,229.05 764.78,233.049 759.84,233.049 C754.91,233.049 750.91,229.05 750.91,224.116 C750.91,219.183 754.91,215.184 759.84,215.184 C764.78,215.184 768.78,219.183 768.78,224.116" id="Fill-953" sketch:type="MSShapeGroup"></path><path d="M768.78,250.915 C768.78,255.848 764.78,259.848 759.84,259.848 C754.91,259.848 750.91,255.848 750.91,250.915 C750.91,245.981 754.91,241.982 759.84,241.982 C764.78,241.982 768.78,245.981 768.78,250.915" id="Fill-954" sketch:type="MSShapeGroup"></path><path d="M768.78,277.713 C768.78,282.647 764.78,286.646 759.84,286.646 C754.91,286.646 750.91,282.647 750.91,277.713 C750.91,272.78 754.91,268.78 759.84,268.78 C764.78,268.78 768.78,272.78 768.78,277.713" id="Fill-955" sketch:type="MSShapeGroup"></path><path d="M768.78,304.512 C768.78,309.445 764.78,313.445 759.84,313.445 C754.91,313.445 750.91,309.445 750.91,304.512 C750.91,299.578 754.91,295.579 759.84,295.579 C764.78,295.579 768.78,299.578 768.78,304.512" id="Fill-956" sketch:type="MSShapeGroup"></path><path d="M768.78,331.31 C768.78,336.244 764.78,340.243 759.84,340.243 C754.91,340.243 750.91,336.244 750.91,331.31 C750.91,326.377 754.91,322.378 759.84,322.378 C764.78,322.378 768.78,326.377 768.78,331.31" id="Fill-957" sketch:type="MSShapeGroup"></path><path d="M768.78,358.109 C768.78,363.042 764.78,367.042 759.84,367.042 C754.91,367.042 750.91,363.042 750.91,358.109 C750.91,353.175 754.91,349.176 759.84,349.176 C764.78,349.176 768.78,353.175 768.78,358.109" id="Fill-958" sketch:type="MSShapeGroup"></path><path d="M768.78,384.907 C768.78,389.841 764.78,393.84 759.84,393.84 C754.91,393.84 750.91,389.841 750.91,384.907 C750.91,379.974 754.91,375.975 759.84,375.975 C764.78,375.975 768.78,379.974 768.78,384.907" id="Fill-959" sketch:type="MSShapeGroup"></path><path d="M768.78,438.504 C768.78,443.438 764.78,447.437 759.84,447.437 C754.91,447.437 750.91,443.438 750.91,438.504 C750.91,433.571 754.91,429.572 759.84,429.572 C764.78,429.572 768.78,433.571 768.78,438.504" id="Fill-960" sketch:type="MSShapeGroup"></path><path d="M768.78,465.303 C768.78,470.236 764.78,474.236 759.84,474.236 C754.91,474.236 750.91,470.236 750.91,465.303 C750.91,460.369 754.91,456.37 759.84,456.37 C764.78,456.37 768.78,460.369 768.78,465.303" id="Fill-961" sketch:type="MSShapeGroup"></path><path d="M768.78,492.101 C768.78,497.035 764.78,501.034 759.84,501.034 C754.91,501.034 750.91,497.035 750.91,492.101 C750.91,487.168 754.91,483.168 759.84,483.168 C764.78,483.168 768.78,487.168 768.78,492.101" id="Fill-962" sketch:type="MSShapeGroup"></path><path d="M768.78,518.9 C768.78,523.833 764.78,527.833 759.84,527.833 C754.91,527.833 750.91,523.833 750.91,518.9 C750.91,513.966 754.91,509.967 759.84,509.967 C764.78,509.967 768.78,513.966 768.78,518.9" id="Fill-963" sketch:type="MSShapeGroup"></path><path d="M795.57,9.729 C795.57,14.662 791.57,18.661 786.64,18.661 C781.71,18.661 777.71,14.662 777.71,9.729 C777.71,4.795 781.71,0.796 786.64,0.796 C791.57,0.796 795.57,4.795 795.57,9.729" id="Fill-964" sketch:type="MSShapeGroup"></path><path d="M795.57,36.527 C795.57,41.46 791.57,45.46 786.64,45.46 C781.71,45.46 777.71,41.46 777.71,36.527 C777.71,31.593 781.71,27.594 786.64,27.594 C791.57,27.594 795.57,31.593 795.57,36.527" id="Fill-965" sketch:type="MSShapeGroup"></path><path d="M795.57,63.325 C795.57,68.259 791.57,72.258 786.64,72.258 C781.71,72.258 777.71,68.259 777.71,63.325 C777.71,58.392 781.71,54.392 786.64,54.392 C791.57,54.392 795.57,58.392 795.57,63.325" id="Fill-966" sketch:type="MSShapeGroup"></path><path d="M795.57,90.124 C795.57,95.057 791.57,99.057 786.64,99.057 C781.71,99.057 777.71,95.057 777.71,90.124 C777.71,85.19 781.71,81.191 786.64,81.191 C791.57,81.191 795.57,85.19 795.57,90.124" id="Fill-967" sketch:type="MSShapeGroup"></path><path d="M795.57,116.922 C795.57,121.856 791.57,125.855 786.64,125.855 C781.71,125.855 777.71,121.856 777.71,116.922 C777.71,111.989 781.71,107.989 786.64,107.989 C791.57,107.989 795.57,111.989 795.57,116.922" id="Fill-968" sketch:type="MSShapeGroup"></path><path d="M795.57,143.721 C795.57,148.654 791.57,152.654 786.64,152.654 C781.71,152.654 777.71,148.654 777.71,143.721 C777.71,138.787 781.71,134.788 786.64,134.788 C791.57,134.788 795.57,138.787 795.57,143.721" id="Fill-969" sketch:type="MSShapeGroup"></path><path d="M795.57,170.519 C795.57,175.453 791.57,179.452 786.64,179.452 C781.71,179.452 777.71,175.453 777.71,170.519 C777.71,165.586 781.71,161.586 786.64,161.586 C791.57,161.586 795.57,165.586 795.57,170.519" id="Fill-970" sketch:type="MSShapeGroup"></path><path d="M795.57,197.318 C795.57,202.251 791.57,206.251 786.64,206.251 C781.71,206.251 777.71,202.251 777.71,197.318 C777.71,192.384 781.71,188.385 786.64,188.385 C791.57,188.385 795.57,192.384 795.57,197.318" id="Fill-971" sketch:type="MSShapeGroup"></path><path d="M795.57,224.116 C795.57,229.05 791.57,233.049 786.64,233.049 C781.71,233.049 777.71,229.05 777.71,224.116 C777.71,219.183 781.71,215.184 786.64,215.184 C791.57,215.184 795.57,219.183 795.57,224.116" id="Fill-972" sketch:type="MSShapeGroup"></path><path d="M795.57,250.915 C795.57,255.848 791.57,259.848 786.64,259.848 C781.71,259.848 777.71,255.848 777.71,250.915 C777.71,245.981 781.71,241.982 786.64,241.982 C791.57,241.982 795.57,245.981 795.57,250.915" id="Fill-973" sketch:type="MSShapeGroup"></path><path d="M795.57,277.713 C795.57,282.647 791.57,286.646 786.64,286.646 C781.71,286.646 777.71,282.647 777.71,277.713 C777.71,272.78 781.71,268.78 786.64,268.78 C791.57,268.78 795.57,272.78 795.57,277.713" id="Fill-974" sketch:type="MSShapeGroup"></path><path d="M795.57,304.512 C795.57,309.445 791.57,313.445 786.64,313.445 C781.71,313.445 777.71,309.445 777.71,304.512 C777.71,299.578 781.71,295.579 786.64,295.579 C791.57,295.579 795.57,299.578 795.57,304.512" id="Fill-975" sketch:type="MSShapeGroup"></path><path d="M795.57,331.31 C795.57,336.244 791.57,340.243 786.64,340.243 C781.71,340.243 777.71,336.244 777.71,331.31 C777.71,326.377 781.71,322.378 786.64,322.378 C791.57,322.378 795.57,326.377 795.57,331.31" id="Fill-976" sketch:type="MSShapeGroup"></path><path d="M795.57,358.109 C795.57,363.042 791.57,367.042 786.64,367.042 C781.71,367.042 777.71,363.042 777.71,358.109 C777.71,353.175 781.71,349.176 786.64,349.176 C791.57,349.176 795.57,353.175 795.57,358.109" id="Fill-977" sketch:type="MSShapeGroup"></path><path d="M795.57,384.907 C795.57,389.841 791.57,393.84 786.64,393.84 C781.71,393.84 777.71,389.841 777.71,384.907 C777.71,379.974 781.71,375.975 786.64,375.975 C791.57,375.975 795.57,379.974 795.57,384.907" id="Fill-978" sketch:type="MSShapeGroup"></path><path d="M795.57,411.706 C795.57,416.639 791.57,420.639 786.64,420.639 C781.71,420.639 777.71,416.639 777.71,411.706 C777.71,406.772 781.71,402.773 786.64,402.773 C791.57,402.773 795.57,406.772 795.57,411.706" id="Fill-979" sketch:type="MSShapeGroup"></path><path d="M795.57,438.504 C795.57,443.438 791.57,447.437 786.64,447.437 C781.71,447.437 777.71,443.438 777.71,438.504 C777.71,433.571 781.71,429.572 786.64,429.572 C791.57,429.572 795.57,433.571 795.57,438.504" id="Fill-980" sketch:type="MSShapeGroup"></path><path d="M795.57,545.698 C795.57,550.632 791.57,554.631 786.64,554.631 C781.71,554.631 777.71,550.632 777.71,545.698 C777.71,540.765 781.71,536.766 786.64,536.766 C791.57,536.766 795.57,540.765 795.57,545.698" id="Fill-981" sketch:type="MSShapeGroup"></path><path d="M795.57,572.497 C795.57,577.43 791.57,581.43 786.64,581.43 C781.71,581.43 777.71,577.43 777.71,572.497 C777.71,567.563 781.71,563.564 786.64,563.564 C791.57,563.564 795.57,567.563 795.57,572.497" id="Fill-982" sketch:type="MSShapeGroup"></path><path d="M795.57,626.094 C795.57,631.027 791.57,635.027 786.64,635.027 C781.71,635.027 777.71,631.027 777.71,626.094 C777.71,621.16 781.71,617.161 786.64,617.161 C791.57,617.161 795.57,621.16 795.57,626.094" id="Fill-983" sketch:type="MSShapeGroup"></path><path d="M795.57,652.892 C795.57,657.826 791.57,661.825 786.64,661.825 C781.71,661.825 777.71,657.826 777.71,652.892 C777.71,647.959 781.71,643.96 786.64,643.96 C791.57,643.96 795.57,647.959 795.57,652.892" id="Fill-984" sketch:type="MSShapeGroup"></path><path d="M822.37,36.527 C822.37,41.46 818.37,45.46 813.44,45.46 C808.51,45.46 804.51,41.46 804.51,36.527 C804.51,31.593 808.51,27.594 813.44,27.594 C818.37,27.594 822.37,31.593 822.37,36.527" id="Fill-985" sketch:type="MSShapeGroup"></path><path d="M822.37,63.325 C822.37,68.259 818.37,72.258 813.44,72.258 C808.51,72.258 804.51,68.259 804.51,63.325 C804.51,58.392 808.51,54.392 813.44,54.392 C818.37,54.392 822.37,58.392 822.37,63.325" id="Fill-986" sketch:type="MSShapeGroup"></path><path d="M822.37,90.124 C822.37,95.057 818.37,99.057 813.44,99.057 C808.51,99.057 804.51,95.057 804.51,90.124 C804.51,85.19 808.51,81.191 813.44,81.191 C818.37,81.191 822.37,85.19 822.37,90.124" id="Fill-987" sketch:type="MSShapeGroup"></path><path d="M822.37,116.922 C822.37,121.856 818.37,125.855 813.44,125.855 C808.51,125.855 804.51,121.856 804.51,116.922 C804.51,111.989 808.51,107.989 813.44,107.989 C818.37,107.989 822.37,111.989 822.37,116.922" id="Fill-988" sketch:type="MSShapeGroup"></path><path d="M822.37,143.721 C822.37,148.654 818.37,152.654 813.44,152.654 C808.51,152.654 804.51,148.654 804.51,143.721 C804.51,138.787 808.51,134.788 813.44,134.788 C818.37,134.788 822.37,138.787 822.37,143.721" id="Fill-989" sketch:type="MSShapeGroup"></path><path d="M822.37,170.519 C822.37,175.453 818.37,179.452 813.44,179.452 C808.51,179.452 804.51,175.453 804.51,170.519 C804.51,165.586 808.51,161.586 813.44,161.586 C818.37,161.586 822.37,165.586 822.37,170.519" id="Fill-990" sketch:type="MSShapeGroup"></path><path d="M822.37,197.318 C822.37,202.251 818.37,206.251 813.44,206.251 C808.51,206.251 804.51,202.251 804.51,197.318 C804.51,192.384 808.51,188.385 813.44,188.385 C818.37,188.385 822.37,192.384 822.37,197.318" id="Fill-991" sketch:type="MSShapeGroup"></path><path d="M822.37,224.116 C822.37,229.05 818.37,233.049 813.44,233.049 C808.51,233.049 804.51,229.05 804.51,224.116 C804.51,219.183 808.51,215.184 813.44,215.184 C818.37,215.184 822.37,219.183 822.37,224.116" id="Fill-992" sketch:type="MSShapeGroup"></path><path d="M822.37,250.915 C822.37,255.848 818.37,259.848 813.44,259.848 C808.51,259.848 804.51,255.848 804.51,250.915 C804.51,245.981 808.51,241.982 813.44,241.982 C818.37,241.982 822.37,245.981 822.37,250.915" id="Fill-993" sketch:type="MSShapeGroup"></path><path d="M822.37,277.713 C822.37,282.647 818.37,286.646 813.44,286.646 C808.51,286.646 804.51,282.647 804.51,277.713 C804.51,272.78 808.51,268.78 813.44,268.78 C818.37,268.78 822.37,272.78 822.37,277.713" id="Fill-994" sketch:type="MSShapeGroup"></path><path d="M822.37,304.512 C822.37,309.445 818.37,313.445 813.44,313.445 C808.51,313.445 804.51,309.445 804.51,304.512 C804.51,299.578 808.51,295.579 813.44,295.579 C818.37,295.579 822.37,299.578 822.37,304.512" id="Fill-995" sketch:type="MSShapeGroup"></path><path d="M822.37,331.31 C822.37,336.244 818.37,340.243 813.44,340.243 C808.51,340.243 804.51,336.244 804.51,331.31 C804.51,326.377 808.51,322.378 813.44,322.378 C818.37,322.378 822.37,326.377 822.37,331.31" id="Fill-996" sketch:type="MSShapeGroup"></path><path d="M822.37,358.109 C822.37,363.042 818.37,367.042 813.44,367.042 C808.51,367.042 804.51,363.042 804.51,358.109 C804.51,353.175 808.51,349.176 813.44,349.176 C818.37,349.176 822.37,353.175 822.37,358.109" id="Fill-997" sketch:type="MSShapeGroup"></path><path d="M822.37,384.907 C822.37,389.841 818.37,393.84 813.44,393.84 C808.51,393.84 804.51,389.841 804.51,384.907 C804.51,379.974 808.51,375.975 813.44,375.975 C818.37,375.975 822.37,379.974 822.37,384.907" id="Fill-998" sketch:type="MSShapeGroup"></path><path d="M822.37,411.706 C822.37,416.639 818.37,420.639 813.44,420.639 C808.51,420.639 804.51,416.639 804.51,411.706 C804.51,406.772 808.51,402.773 813.44,402.773 C818.37,402.773 822.37,406.772 822.37,411.706" id="Fill-999" sketch:type="MSShapeGroup"></path><path d="M822.37,438.504 C822.37,443.438 818.37,447.437 813.44,447.437 C808.51,447.437 804.51,443.438 804.51,438.504 C804.51,433.571 808.51,429.572 813.44,429.572 C818.37,429.572 822.37,433.571 822.37,438.504" id="Fill-1000" sketch:type="MSShapeGroup"></path><path d="M822.37,545.698 C822.37,550.632 818.37,554.631 813.44,554.631 C808.51,554.631 804.51,550.632 804.51,545.698 C804.51,540.765 808.51,536.766 813.44,536.766 C818.37,536.766 822.37,540.765 822.37,545.698" id="Fill-1001" sketch:type="MSShapeGroup"></path><path d="M822.37,572.497 C822.37,577.43 818.37,581.43 813.44,581.43 C808.51,581.43 804.51,577.43 804.51,572.497 C804.51,567.563 808.51,563.564 813.44,563.564 C818.37,563.564 822.37,567.563 822.37,572.497" id="Fill-1002" sketch:type="MSShapeGroup"></path><path d="M822.37,652.892 C822.37,657.826 818.37,661.825 813.44,661.825 C808.51,661.825 804.51,657.826 804.51,652.892 C804.51,647.959 808.51,643.96 813.44,643.96 C818.37,643.96 822.37,647.959 822.37,652.892" id="Fill-1003" sketch:type="MSShapeGroup"></path><path d="M822.37,679.691 C822.37,684.624 818.37,688.624 813.44,688.624 C808.51,688.624 804.51,684.624 804.51,679.691 C804.51,674.757 808.51,670.758 813.44,670.758 C818.37,670.758 822.37,674.757 822.37,679.691" id="Fill-1004" sketch:type="MSShapeGroup"></path><path d="M849.17,63.325 C849.17,68.259 845.17,72.258 840.24,72.258 C835.3,72.258 831.31,68.259 831.31,63.325 C831.31,58.392 835.3,54.392 840.24,54.392 C845.17,54.392 849.17,58.392 849.17,63.325" id="Fill-1005" sketch:type="MSShapeGroup"></path><path d="M849.17,90.124 C849.17,95.057 845.17,99.057 840.24,99.057 C835.3,99.057 831.31,95.057 831.31,90.124 C831.31,85.19 835.3,81.191 840.24,81.191 C845.17,81.191 849.17,85.19 849.17,90.124" id="Fill-1006" sketch:type="MSShapeGroup"></path><path d="M849.17,116.922 C849.17,121.856 845.17,125.855 840.24,125.855 C835.3,125.855 831.31,121.856 831.31,116.922 C831.31,111.989 835.3,107.989 840.24,107.989 C845.17,107.989 849.17,111.989 849.17,116.922" id="Fill-1007" sketch:type="MSShapeGroup"></path><path d="M849.17,143.721 C849.17,148.654 845.17,152.654 840.24,152.654 C835.3,152.654 831.31,148.654 831.31,143.721 C831.31,138.787 835.3,134.788 840.24,134.788 C845.17,134.788 849.17,138.787 849.17,143.721" id="Fill-1008" sketch:type="MSShapeGroup"></path><path d="M849.17,170.519 C849.17,175.453 845.17,179.452 840.24,179.452 C835.3,179.452 831.31,175.453 831.31,170.519 C831.31,165.586 835.3,161.586 840.24,161.586 C845.17,161.586 849.17,165.586 849.17,170.519" id="Fill-1009" sketch:type="MSShapeGroup"></path><path d="M849.17,197.318 C849.17,202.251 845.17,206.251 840.24,206.251 C835.3,206.251 831.31,202.251 831.31,197.318 C831.31,192.384 835.3,188.385 840.24,188.385 C845.17,188.385 849.17,192.384 849.17,197.318" id="Fill-1010" sketch:type="MSShapeGroup"></path><path d="M849.17,224.116 C849.17,229.05 845.17,233.049 840.24,233.049 C835.3,233.049 831.31,229.05 831.31,224.116 C831.31,219.183 835.3,215.184 840.24,215.184 C845.17,215.184 849.17,219.183 849.17,224.116" id="Fill-1011" sketch:type="MSShapeGroup"></path><path d="M849.17,250.915 C849.17,255.848 845.17,259.848 840.24,259.848 C835.3,259.848 831.31,255.848 831.31,250.915 C831.31,245.981 835.3,241.982 840.24,241.982 C845.17,241.982 849.17,245.981 849.17,250.915" id="Fill-1012" sketch:type="MSShapeGroup"></path><path d="M849.17,277.713 C849.17,282.647 845.17,286.646 840.24,286.646 C835.3,286.646 831.31,282.647 831.31,277.713 C831.31,272.78 835.3,268.78 840.24,268.78 C845.17,268.78 849.17,272.78 849.17,277.713" id="Fill-1013" sketch:type="MSShapeGroup"></path><path d="M849.17,304.512 C849.17,309.445 845.17,313.445 840.24,313.445 C835.3,313.445 831.31,309.445 831.31,304.512 C831.31,299.578 835.3,295.579 840.24,295.579 C845.17,295.579 849.17,299.578 849.17,304.512" id="Fill-1014" sketch:type="MSShapeGroup"></path><path d="M849.17,331.31 C849.17,336.244 845.17,340.243 840.24,340.243 C835.3,340.243 831.31,336.244 831.31,331.31 C831.31,326.377 835.3,322.378 840.24,322.378 C845.17,322.378 849.17,326.377 849.17,331.31" id="Fill-1015" sketch:type="MSShapeGroup"></path><path d="M849.17,492.101 C849.17,497.035 845.17,501.034 840.24,501.034 C835.3,501.034 831.31,497.035 831.31,492.101 C831.31,487.168 835.3,483.168 840.24,483.168 C845.17,483.168 849.17,487.168 849.17,492.101" id="Fill-1016" sketch:type="MSShapeGroup"></path><path d="M849.17,518.9 C849.17,523.833 845.17,527.833 840.24,527.833 C835.3,527.833 831.31,523.833 831.31,518.9 C831.31,513.966 835.3,509.967 840.24,509.967 C845.17,509.967 849.17,513.966 849.17,518.9" id="Fill-1017" sketch:type="MSShapeGroup"></path><path d="M849.17,545.698 C849.17,550.632 845.17,554.631 840.24,554.631 C835.3,554.631 831.31,550.632 831.31,545.698 C831.31,540.765 835.3,536.766 840.24,536.766 C845.17,536.766 849.17,540.765 849.17,545.698" id="Fill-1018" sketch:type="MSShapeGroup"></path><path d="M849.17,572.497 C849.17,577.43 845.17,581.43 840.24,581.43 C835.3,581.43 831.31,577.43 831.31,572.497 C831.31,567.563 835.3,563.564 840.24,563.564 C845.17,563.564 849.17,567.563 849.17,572.497" id="Fill-1019" sketch:type="MSShapeGroup"></path><path d="M849.17,626.094 C849.17,631.027 845.17,635.027 840.24,635.027 C835.3,635.027 831.31,631.027 831.31,626.094 C831.31,621.16 835.3,617.161 840.24,617.161 C845.17,617.161 849.17,621.16 849.17,626.094" id="Fill-1020" sketch:type="MSShapeGroup"></path><path d="M849.17,652.892 C849.17,657.826 845.17,661.825 840.24,661.825 C835.3,661.825 831.31,657.826 831.31,652.892 C831.31,647.959 835.3,643.96 840.24,643.96 C845.17,643.96 849.17,647.959 849.17,652.892" id="Fill-1021" sketch:type="MSShapeGroup"></path><path d="M849.17,679.691 C849.17,684.624 845.17,688.624 840.24,688.624 C835.3,688.624 831.31,684.624 831.31,679.691 C831.31,674.757 835.3,670.758 840.24,670.758 C845.17,670.758 849.17,674.757 849.17,679.691" id="Fill-1022" sketch:type="MSShapeGroup"></path><path d="M875.97,63.325 C875.97,68.259 871.97,72.258 867.04,72.258 C862.1,72.258 858.1,68.259 858.1,63.325 C858.1,58.392 862.1,54.392 867.04,54.392 C871.97,54.392 875.97,58.392 875.97,63.325" id="Fill-1023" sketch:type="MSShapeGroup"></path><path d="M875.97,90.124 C875.97,95.057 871.97,99.057 867.04,99.057 C862.1,99.057 858.1,95.057 858.1,90.124 C858.1,85.19 862.1,81.191 867.04,81.191 C871.97,81.191 875.97,85.19 875.97,90.124" id="Fill-1024" sketch:type="MSShapeGroup"></path><path d="M875.97,116.922 C875.97,121.856 871.97,125.855 867.04,125.855 C862.1,125.855 858.1,121.856 858.1,116.922 C858.1,111.989 862.1,107.989 867.04,107.989 C871.97,107.989 875.97,111.989 875.97,116.922" id="Fill-1025" sketch:type="MSShapeGroup"></path><path d="M875.97,143.721 C875.97,148.654 871.97,152.654 867.04,152.654 C862.1,152.654 858.1,148.654 858.1,143.721 C858.1,138.787 862.1,134.788 867.04,134.788 C871.97,134.788 875.97,138.787 875.97,143.721" id="Fill-1026" sketch:type="MSShapeGroup"></path><path d="M875.97,170.519 C875.97,175.453 871.97,179.452 867.04,179.452 C862.1,179.452 858.1,175.453 858.1,170.519 C858.1,165.586 862.1,161.586 867.04,161.586 C871.97,161.586 875.97,165.586 875.97,170.519" id="Fill-1027" sketch:type="MSShapeGroup"></path><path d="M875.97,197.318 C875.97,202.251 871.97,206.251 867.04,206.251 C862.1,206.251 858.1,202.251 858.1,197.318 C858.1,192.384 862.1,188.385 867.04,188.385 C871.97,188.385 875.97,192.384 875.97,197.318" id="Fill-1028" sketch:type="MSShapeGroup"></path><path d="M875.97,224.116 C875.97,229.05 871.97,233.049 867.04,233.049 C862.1,233.049 858.1,229.05 858.1,224.116 C858.1,219.183 862.1,215.184 867.04,215.184 C871.97,215.184 875.97,219.183 875.97,224.116" id="Fill-1029" sketch:type="MSShapeGroup"></path><path d="M875.97,250.915 C875.97,255.848 871.97,259.848 867.04,259.848 C862.1,259.848 858.1,255.848 858.1,250.915 C858.1,245.981 862.1,241.982 867.04,241.982 C871.97,241.982 875.97,245.981 875.97,250.915" id="Fill-1030" sketch:type="MSShapeGroup"></path><path d="M875.97,277.713 C875.97,282.647 871.97,286.646 867.04,286.646 C862.1,286.646 858.1,282.647 858.1,277.713 C858.1,272.78 862.1,268.78 867.04,268.78 C871.97,268.78 875.97,272.78 875.97,277.713" id="Fill-1031" sketch:type="MSShapeGroup"></path><path d="M875.97,304.512 C875.97,309.445 871.97,313.445 867.04,313.445 C862.1,313.445 858.1,309.445 858.1,304.512 C858.1,299.578 862.1,295.579 867.04,295.579 C871.97,295.579 875.97,299.578 875.97,304.512" id="Fill-1032" sketch:type="MSShapeGroup"></path><path d="M875.97,331.31 C875.97,336.244 871.97,340.243 867.04,340.243 C862.1,340.243 858.1,336.244 858.1,331.31 C858.1,326.377 862.1,322.378 867.04,322.378 C871.97,322.378 875.97,326.377 875.97,331.31" id="Fill-1033" sketch:type="MSShapeGroup"></path><path d="M875.97,545.698 C875.97,550.632 871.97,554.631 867.04,554.631 C862.1,554.631 858.1,550.632 858.1,545.698 C858.1,540.765 862.1,536.766 867.04,536.766 C871.97,536.766 875.97,540.765 875.97,545.698" id="Fill-1034" sketch:type="MSShapeGroup"></path><path d="M875.97,626.094 C875.97,631.027 871.97,635.027 867.04,635.027 C862.1,635.027 858.1,631.027 858.1,626.094 C858.1,621.16 862.1,617.161 867.04,617.161 C871.97,617.161 875.97,621.16 875.97,626.094" id="Fill-1035" sketch:type="MSShapeGroup"></path><path d="M875.97,652.892 C875.97,657.826 871.97,661.825 867.04,661.825 C862.1,661.825 858.1,657.826 858.1,652.892 C858.1,647.959 862.1,643.96 867.04,643.96 C871.97,643.96 875.97,647.959 875.97,652.892" id="Fill-1036" sketch:type="MSShapeGroup"></path><path d="M875.97,760.086 C875.97,765.02 871.97,769.019 867.04,769.019 C862.1,769.019 858.1,765.02 858.1,760.086 C858.1,755.153 862.1,751.154 867.04,751.154 C871.97,751.154 875.97,755.153 875.97,760.086" id="Fill-1037" sketch:type="MSShapeGroup"></path><path d="M875.97,786.885 C875.97,791.818 871.97,795.818 867.04,795.818 C862.1,795.818 858.1,791.818 858.1,786.885 C858.1,781.951 862.1,777.952 867.04,777.952 C871.97,777.952 875.97,781.951 875.97,786.885" id="Fill-1038" sketch:type="MSShapeGroup"></path><path d="M875.97,813.683 C875.97,818.617 871.97,822.616 867.04,822.616 C862.1,822.616 858.1,818.617 858.1,813.683 C858.1,808.75 862.1,804.751 867.04,804.751 C871.97,804.751 875.97,808.75 875.97,813.683" id="Fill-1039" sketch:type="MSShapeGroup"></path><path d="M875.97,840.482 C875.97,845.415 871.97,849.415 867.04,849.415 C862.1,849.415 858.1,845.415 858.1,840.482 C858.1,835.548 862.1,831.549 867.04,831.549 C871.97,831.549 875.97,835.548 875.97,840.482" id="Fill-1040" sketch:type="MSShapeGroup"></path><path d="M902.77,63.325 C902.77,68.259 898.77,72.258 893.84,72.258 C888.9,72.258 884.9,68.259 884.9,63.325 C884.9,58.392 888.9,54.392 893.84,54.392 C898.77,54.392 902.77,58.392 902.77,63.325" id="Fill-1041" sketch:type="MSShapeGroup"></path><path d="M902.77,90.124 C902.77,95.057 898.77,99.057 893.84,99.057 C888.9,99.057 884.9,95.057 884.9,90.124 C884.9,85.19 888.9,81.191 893.84,81.191 C898.77,81.191 902.77,85.19 902.77,90.124" id="Fill-1042" sketch:type="MSShapeGroup"></path><path d="M902.77,116.922 C902.77,121.856 898.77,125.855 893.84,125.855 C888.9,125.855 884.9,121.856 884.9,116.922 C884.9,111.989 888.9,107.989 893.84,107.989 C898.77,107.989 902.77,111.989 902.77,116.922" id="Fill-1043" sketch:type="MSShapeGroup"></path><path d="M902.77,143.721 C902.77,148.654 898.77,152.654 893.84,152.654 C888.9,152.654 884.9,148.654 884.9,143.721 C884.9,138.787 888.9,134.788 893.84,134.788 C898.77,134.788 902.77,138.787 902.77,143.721" id="Fill-1044" sketch:type="MSShapeGroup"></path><path d="M902.77,170.519 C902.77,175.453 898.77,179.452 893.84,179.452 C888.9,179.452 884.9,175.453 884.9,170.519 C884.9,165.586 888.9,161.586 893.84,161.586 C898.77,161.586 902.77,165.586 902.77,170.519" id="Fill-1045" sketch:type="MSShapeGroup"></path><path d="M902.77,197.318 C902.77,202.251 898.77,206.251 893.84,206.251 C888.9,206.251 884.9,202.251 884.9,197.318 C884.9,192.384 888.9,188.385 893.84,188.385 C898.77,188.385 902.77,192.384 902.77,197.318" id="Fill-1046" sketch:type="MSShapeGroup"></path><path d="M902.77,224.116 C902.77,229.05 898.77,233.049 893.84,233.049 C888.9,233.049 884.9,229.05 884.9,224.116 C884.9,219.183 888.9,215.184 893.84,215.184 C898.77,215.184 902.77,219.183 902.77,224.116" id="Fill-1047" sketch:type="MSShapeGroup"></path><path d="M902.77,250.915 C902.77,255.848 898.77,259.848 893.84,259.848 C888.9,259.848 884.9,255.848 884.9,250.915 C884.9,245.981 888.9,241.982 893.84,241.982 C898.77,241.982 902.77,245.981 902.77,250.915" id="Fill-1048" sketch:type="MSShapeGroup"></path><path d="M902.77,277.713 C902.77,282.647 898.77,286.646 893.84,286.646 C888.9,286.646 884.9,282.647 884.9,277.713 C884.9,272.78 888.9,268.78 893.84,268.78 C898.77,268.78 902.77,272.78 902.77,277.713" id="Fill-1049" sketch:type="MSShapeGroup"></path><path d="M902.77,384.907 C902.77,389.841 898.77,393.84 893.84,393.84 C888.9,393.84 884.9,389.841 884.9,384.907 C884.9,379.974 888.9,375.975 893.84,375.975 C898.77,375.975 902.77,379.974 902.77,384.907" id="Fill-1050" sketch:type="MSShapeGroup"></path><path d="M902.77,626.094 C902.77,631.027 898.77,635.027 893.84,635.027 C888.9,635.027 884.9,631.027 884.9,626.094 C884.9,621.16 888.9,617.161 893.84,617.161 C898.77,617.161 902.77,621.16 902.77,626.094" id="Fill-1051" sketch:type="MSShapeGroup"></path><path d="M902.77,652.892 C902.77,657.826 898.77,661.825 893.84,661.825 C888.9,661.825 884.9,657.826 884.9,652.892 C884.9,647.959 888.9,643.96 893.84,643.96 C898.77,643.96 902.77,647.959 902.77,652.892" id="Fill-1052" sketch:type="MSShapeGroup"></path><path d="M902.77,733.288 C902.77,738.221 898.77,742.221 893.84,742.221 C888.9,742.221 884.9,738.221 884.9,733.288 C884.9,728.354 888.9,724.355 893.84,724.355 C898.77,724.355 902.77,728.354 902.77,733.288" id="Fill-1053" sketch:type="MSShapeGroup"></path><path d="M902.77,760.086 C902.77,765.02 898.77,769.019 893.84,769.019 C888.9,769.019 884.9,765.02 884.9,760.086 C884.9,755.153 888.9,751.154 893.84,751.154 C898.77,751.154 902.77,755.153 902.77,760.086" id="Fill-1054" sketch:type="MSShapeGroup"></path><path d="M902.77,786.885 C902.77,791.818 898.77,795.818 893.84,795.818 C888.9,795.818 884.9,791.818 884.9,786.885 C884.9,781.951 888.9,777.952 893.84,777.952 C898.77,777.952 902.77,781.951 902.77,786.885" id="Fill-1055" sketch:type="MSShapeGroup"></path><path d="M902.77,813.683 C902.77,818.617 898.77,822.616 893.84,822.616 C888.9,822.616 884.9,818.617 884.9,813.683 C884.9,808.75 888.9,804.751 893.84,804.751 C898.77,804.751 902.77,808.75 902.77,813.683" id="Fill-1056" sketch:type="MSShapeGroup"></path><path d="M902.77,840.482 C902.77,845.415 898.77,849.415 893.84,849.415 C888.9,849.415 884.9,845.415 884.9,840.482 C884.9,835.548 888.9,831.549 893.84,831.549 C898.77,831.549 902.77,835.548 902.77,840.482" id="Fill-1057" sketch:type="MSShapeGroup"></path><path d="M929.57,63.325 C929.57,68.259 925.57,72.258 920.63,72.258 C915.7,72.258 911.7,68.259 911.7,63.325 C911.7,58.392 915.7,54.392 920.63,54.392 C925.57,54.392 929.57,58.392 929.57,63.325" id="Fill-1058" sketch:type="MSShapeGroup"></path><path d="M929.57,90.124 C929.57,95.057 925.57,99.057 920.63,99.057 C915.7,99.057 911.7,95.057 911.7,90.124 C911.7,85.19 915.7,81.191 920.63,81.191 C925.57,81.191 929.57,85.19 929.57,90.124" id="Fill-1059" sketch:type="MSShapeGroup"></path><path d="M929.57,116.922 C929.57,121.856 925.57,125.855 920.63,125.855 C915.7,125.855 911.7,121.856 911.7,116.922 C911.7,111.989 915.7,107.989 920.63,107.989 C925.57,107.989 929.57,111.989 929.57,116.922" id="Fill-1060" sketch:type="MSShapeGroup"></path><path d="M929.57,143.721 C929.57,148.654 925.57,152.654 920.63,152.654 C915.7,152.654 911.7,148.654 911.7,143.721 C911.7,138.787 915.7,134.788 920.63,134.788 C925.57,134.788 929.57,138.787 929.57,143.721" id="Fill-1061" sketch:type="MSShapeGroup"></path><path d="M929.57,170.519 C929.57,175.453 925.57,179.452 920.63,179.452 C915.7,179.452 911.7,175.453 911.7,170.519 C911.7,165.586 915.7,161.586 920.63,161.586 C925.57,161.586 929.57,165.586 929.57,170.519" id="Fill-1062" sketch:type="MSShapeGroup"></path><path d="M929.57,197.318 C929.57,202.251 925.57,206.251 920.63,206.251 C915.7,206.251 911.7,202.251 911.7,197.318 C911.7,192.384 915.7,188.385 920.63,188.385 C925.57,188.385 929.57,192.384 929.57,197.318" id="Fill-1063" sketch:type="MSShapeGroup"></path><path d="M929.57,224.116 C929.57,229.05 925.57,233.049 920.63,233.049 C915.7,233.049 911.7,229.05 911.7,224.116 C911.7,219.183 915.7,215.184 920.63,215.184 C925.57,215.184 929.57,219.183 929.57,224.116" id="Fill-1064" sketch:type="MSShapeGroup"></path><path d="M929.57,250.915 C929.57,255.848 925.57,259.848 920.63,259.848 C915.7,259.848 911.7,255.848 911.7,250.915 C911.7,245.981 915.7,241.982 920.63,241.982 C925.57,241.982 929.57,245.981 929.57,250.915" id="Fill-1065" sketch:type="MSShapeGroup"></path><path d="M929.57,358.109 C929.57,363.042 925.57,367.042 920.63,367.042 C915.7,367.042 911.7,363.042 911.7,358.109 C911.7,353.175 915.7,349.176 920.63,349.176 C925.57,349.176 929.57,353.175 929.57,358.109" id="Fill-1066" sketch:type="MSShapeGroup"></path><path d="M929.57,384.907 C929.57,389.841 925.57,393.84 920.63,393.84 C915.7,393.84 911.7,389.841 911.7,384.907 C911.7,379.974 915.7,375.975 920.63,375.975 C925.57,375.975 929.57,379.974 929.57,384.907" id="Fill-1067" sketch:type="MSShapeGroup"></path><path d="M929.57,626.094 C929.57,631.027 925.57,635.027 920.63,635.027 C915.7,635.027 911.7,631.027 911.7,626.094 C911.7,621.16 915.7,617.161 920.63,617.161 C925.57,617.161 929.57,621.16 929.57,626.094" id="Fill-1068" sketch:type="MSShapeGroup"></path><path d="M929.57,733.288 C929.57,738.221 925.57,742.221 920.63,742.221 C915.7,742.221 911.7,738.221 911.7,733.288 C911.7,728.354 915.7,724.355 920.63,724.355 C925.57,724.355 929.57,728.354 929.57,733.288" id="Fill-1069" sketch:type="MSShapeGroup"></path><path d="M929.57,760.086 C929.57,765.02 925.57,769.019 920.63,769.019 C915.7,769.019 911.7,765.02 911.7,760.086 C911.7,755.153 915.7,751.154 920.63,751.154 C925.57,751.154 929.57,755.153 929.57,760.086" id="Fill-1070" sketch:type="MSShapeGroup"></path><path d="M929.57,786.885 C929.57,791.818 925.57,795.818 920.63,795.818 C915.7,795.818 911.7,791.818 911.7,786.885 C911.7,781.951 915.7,777.952 920.63,777.952 C925.57,777.952 929.57,781.951 929.57,786.885" id="Fill-1071" sketch:type="MSShapeGroup"></path><path d="M929.57,813.683 C929.57,818.617 925.57,822.616 920.63,822.616 C915.7,822.616 911.7,818.617 911.7,813.683 C911.7,808.75 915.7,804.751 920.63,804.751 C925.57,804.751 929.57,808.75 929.57,813.683" id="Fill-1072" sketch:type="MSShapeGroup"></path><path d="M929.57,840.482 C929.57,845.415 925.57,849.415 920.63,849.415 C915.7,849.415 911.7,845.415 911.7,840.482 C911.7,835.548 915.7,831.549 920.63,831.549 C925.57,831.549 929.57,835.548 929.57,840.482" id="Fill-1073" sketch:type="MSShapeGroup"></path><path d="M956.36,63.325 C956.36,68.259 952.37,72.258 947.43,72.258 C942.5,72.258 938.5,68.259 938.5,63.325 C938.5,58.392 942.5,54.392 947.43,54.392 C952.37,54.392 956.36,58.392 956.36,63.325" id="Fill-1074" sketch:type="MSShapeGroup"></path><path d="M956.36,90.124 C956.36,95.057 952.37,99.057 947.43,99.057 C942.5,99.057 938.5,95.057 938.5,90.124 C938.5,85.19 942.5,81.191 947.43,81.191 C952.37,81.191 956.36,85.19 956.36,90.124" id="Fill-1075" sketch:type="MSShapeGroup"></path><path d="M956.36,116.922 C956.36,121.856 952.37,125.855 947.43,125.855 C942.5,125.855 938.5,121.856 938.5,116.922 C938.5,111.989 942.5,107.989 947.43,107.989 C952.37,107.989 956.36,111.989 956.36,116.922" id="Fill-1076" sketch:type="MSShapeGroup"></path><path d="M956.36,143.721 C956.36,148.654 952.37,152.654 947.43,152.654 C942.5,152.654 938.5,148.654 938.5,143.721 C938.5,138.787 942.5,134.788 947.43,134.788 C952.37,134.788 956.36,138.787 956.36,143.721" id="Fill-1077" sketch:type="MSShapeGroup"></path><path d="M956.36,170.519 C956.36,175.453 952.37,179.452 947.43,179.452 C942.5,179.452 938.5,175.453 938.5,170.519 C938.5,165.586 942.5,161.586 947.43,161.586 C952.37,161.586 956.36,165.586 956.36,170.519" id="Fill-1078" sketch:type="MSShapeGroup"></path><path d="M956.36,197.318 C956.36,202.251 952.37,206.251 947.43,206.251 C942.5,206.251 938.5,202.251 938.5,197.318 C938.5,192.384 942.5,188.385 947.43,188.385 C952.37,188.385 956.36,192.384 956.36,197.318" id="Fill-1079" sketch:type="MSShapeGroup"></path><path d="M956.36,224.116 C956.36,229.05 952.37,233.049 947.43,233.049 C942.5,233.049 938.5,229.05 938.5,224.116 C938.5,219.183 942.5,215.184 947.43,215.184 C952.37,215.184 956.36,219.183 956.36,224.116" id="Fill-1080" sketch:type="MSShapeGroup"></path><path d="M956.36,250.915 C956.36,255.848 952.37,259.848 947.43,259.848 C942.5,259.848 938.5,255.848 938.5,250.915 C938.5,245.981 942.5,241.982 947.43,241.982 C952.37,241.982 956.36,245.981 956.36,250.915" id="Fill-1081" sketch:type="MSShapeGroup"></path><path d="M956.36,304.512 C956.36,309.445 952.37,313.445 947.43,313.445 C942.5,313.445 938.5,309.445 938.5,304.512 C938.5,299.578 942.5,295.579 947.43,295.579 C952.37,295.579 956.36,299.578 956.36,304.512" id="Fill-1082" sketch:type="MSShapeGroup"></path><path d="M956.36,331.31 C956.36,336.244 952.37,340.243 947.43,340.243 C942.5,340.243 938.5,336.244 938.5,331.31 C938.5,326.377 942.5,322.378 947.43,322.378 C952.37,322.378 956.36,326.377 956.36,331.31" id="Fill-1083" sketch:type="MSShapeGroup"></path><path d="M956.36,358.109 C956.36,363.042 952.37,367.042 947.43,367.042 C942.5,367.042 938.5,363.042 938.5,358.109 C938.5,353.175 942.5,349.176 947.43,349.176 C952.37,349.176 956.36,353.175 956.36,358.109" id="Fill-1084" sketch:type="MSShapeGroup"></path><path d="M956.36,652.892 C956.36,657.826 952.37,661.825 947.43,661.825 C942.5,661.825 938.5,657.826 938.5,652.892 C938.5,647.959 942.5,643.96 947.43,643.96 C952.37,643.96 956.36,647.959 956.36,652.892" id="Fill-1085" sketch:type="MSShapeGroup"></path><path d="M956.36,706.489 C956.36,711.423 952.37,715.422 947.43,715.422 C942.5,715.422 938.5,711.423 938.5,706.489 C938.5,701.556 942.5,697.556 947.43,697.556 C952.37,697.556 956.36,701.556 956.36,706.489" id="Fill-1086" sketch:type="MSShapeGroup"></path><path d="M956.36,733.288 C956.36,738.221 952.37,742.221 947.43,742.221 C942.5,742.221 938.5,738.221 938.5,733.288 C938.5,728.354 942.5,724.355 947.43,724.355 C952.37,724.355 956.36,728.354 956.36,733.288" id="Fill-1087" sketch:type="MSShapeGroup"></path><path d="M956.36,760.086 C956.36,765.02 952.37,769.019 947.43,769.019 C942.5,769.019 938.5,765.02 938.5,760.086 C938.5,755.153 942.5,751.154 947.43,751.154 C952.37,751.154 956.36,755.153 956.36,760.086" id="Fill-1088" sketch:type="MSShapeGroup"></path><path d="M956.36,786.885 C956.36,791.818 952.37,795.818 947.43,795.818 C942.5,795.818 938.5,791.818 938.5,786.885 C938.5,781.951 942.5,777.952 947.43,777.952 C952.37,777.952 956.36,781.951 956.36,786.885" id="Fill-1089" sketch:type="MSShapeGroup"></path><path d="M956.36,813.683 C956.36,818.617 952.37,822.616 947.43,822.616 C942.5,822.616 938.5,818.617 938.5,813.683 C938.5,808.75 942.5,804.751 947.43,804.751 C952.37,804.751 956.36,808.75 956.36,813.683" id="Fill-1090" sketch:type="MSShapeGroup"></path><path d="M956.36,840.482 C956.36,845.415 952.37,849.415 947.43,849.415 C942.5,849.415 938.5,845.415 938.5,840.482 C938.5,835.548 942.5,831.549 947.43,831.549 C952.37,831.549 956.36,835.548 956.36,840.482" id="Fill-1091" sketch:type="MSShapeGroup"></path><path d="M983.16,90.124 C983.16,95.057 979.16,99.057 974.23,99.057 C969.3,99.057 965.3,95.057 965.3,90.124 C965.3,85.19 969.3,81.191 974.23,81.191 C979.16,81.191 983.16,85.19 983.16,90.124" id="Fill-1092" sketch:type="MSShapeGroup"></path><path d="M983.16,116.922 C983.16,121.856 979.16,125.855 974.23,125.855 C969.3,125.855 965.3,121.856 965.3,116.922 C965.3,111.989 969.3,107.989 974.23,107.989 C979.16,107.989 983.16,111.989 983.16,116.922" id="Fill-1093" sketch:type="MSShapeGroup"></path><path d="M983.16,143.721 C983.16,148.654 979.16,152.654 974.23,152.654 C969.3,152.654 965.3,148.654 965.3,143.721 C965.3,138.787 969.3,134.788 974.23,134.788 C979.16,134.788 983.16,138.787 983.16,143.721" id="Fill-1094" sketch:type="MSShapeGroup"></path><path d="M983.16,170.519 C983.16,175.453 979.16,179.452 974.23,179.452 C969.3,179.452 965.3,175.453 965.3,170.519 C965.3,165.586 969.3,161.586 974.23,161.586 C979.16,161.586 983.16,165.586 983.16,170.519" id="Fill-1095" sketch:type="MSShapeGroup"></path><path d="M983.16,277.713 C983.16,282.647 979.16,286.646 974.23,286.646 C969.3,286.646 965.3,282.647 965.3,277.713 C965.3,272.78 969.3,268.78 974.23,268.78 C979.16,268.78 983.16,272.78 983.16,277.713" id="Fill-1096" sketch:type="MSShapeGroup"></path><path d="M983.16,652.892 C983.16,657.826 979.16,661.825 974.23,661.825 C969.3,661.825 965.3,657.826 965.3,652.892 C965.3,647.959 969.3,643.96 974.23,643.96 C979.16,643.96 983.16,647.959 983.16,652.892" id="Fill-1097" sketch:type="MSShapeGroup"></path><path d="M983.16,733.288 C983.16,738.221 979.16,742.221 974.23,742.221 C969.3,742.221 965.3,738.221 965.3,733.288 C965.3,728.354 969.3,724.355 974.23,724.355 C979.16,724.355 983.16,728.354 983.16,733.288" id="Fill-1098" sketch:type="MSShapeGroup"></path><path d="M983.16,760.086 C983.16,765.02 979.16,769.019 974.23,769.019 C969.3,769.019 965.3,765.02 965.3,760.086 C965.3,755.153 969.3,751.154 974.23,751.154 C979.16,751.154 983.16,755.153 983.16,760.086" id="Fill-1099" sketch:type="MSShapeGroup"></path><path d="M983.16,786.885 C983.16,791.818 979.16,795.818 974.23,795.818 C969.3,795.818 965.3,791.818 965.3,786.885 C965.3,781.951 969.3,777.952 974.23,777.952 C979.16,777.952 983.16,781.951 983.16,786.885" id="Fill-1100" sketch:type="MSShapeGroup"></path><path d="M983.16,813.683 C983.16,818.617 979.16,822.616 974.23,822.616 C969.3,822.616 965.3,818.617 965.3,813.683 C965.3,808.75 969.3,804.751 974.23,804.751 C979.16,804.751 983.16,808.75 983.16,813.683" id="Fill-1101" sketch:type="MSShapeGroup"></path><path d="M983.16,840.482 C983.16,845.415 979.16,849.415 974.23,849.415 C969.3,849.415 965.3,845.415 965.3,840.482 C965.3,835.548 969.3,831.549 974.23,831.549 C979.16,831.549 983.16,835.548 983.16,840.482" id="Fill-1102" sketch:type="MSShapeGroup"></path><path d="M1009.96,90.124 C1009.96,95.057 1005.96,99.057 1001.03,99.057 C996.1,99.057 992.1,95.057 992.1,90.124 C992.1,85.19 996.1,81.191 1001.03,81.191 C1005.96,81.191 1009.96,85.19 1009.96,90.124" id="Fill-1103" sketch:type="MSShapeGroup"></path><path d="M1009.96,116.922 C1009.96,121.856 1005.96,125.855 1001.03,125.855 C996.1,125.855 992.1,121.856 992.1,116.922 C992.1,111.989 996.1,107.989 1001.03,107.989 C1005.96,107.989 1009.96,111.989 1009.96,116.922" id="Fill-1104" sketch:type="MSShapeGroup"></path><path d="M1009.96,143.721 C1009.96,148.654 1005.96,152.654 1001.03,152.654 C996.1,152.654 992.1,148.654 992.1,143.721 C992.1,138.787 996.1,134.788 1001.03,134.788 C1005.96,134.788 1009.96,138.787 1009.96,143.721" id="Fill-1105" sketch:type="MSShapeGroup"></path><path d="M1009.96,170.519 C1009.96,175.453 1005.96,179.452 1001.03,179.452 C996.1,179.452 992.1,175.453 992.1,170.519 C992.1,165.586 996.1,161.586 1001.03,161.586 C1005.96,161.586 1009.96,165.586 1009.96,170.519" id="Fill-1106" sketch:type="MSShapeGroup"></path><path d="M1009.96,652.892 C1009.96,657.826 1005.96,661.825 1001.03,661.825 C996.1,661.825 992.1,657.826 992.1,652.892 C992.1,647.959 996.1,643.96 1001.03,643.96 C1005.96,643.96 1009.96,647.959 1009.96,652.892" id="Fill-1107" sketch:type="MSShapeGroup"></path><path d="M1009.96,679.691 C1009.96,684.624 1005.96,688.624 1001.03,688.624 C996.1,688.624 992.1,684.624 992.1,679.691 C992.1,674.757 996.1,670.758 1001.03,670.758 C1005.96,670.758 1009.96,674.757 1009.96,679.691" id="Fill-1108" sketch:type="MSShapeGroup"></path><path d="M1009.96,706.489 C1009.96,711.423 1005.96,715.422 1001.03,715.422 C996.1,715.422 992.1,711.423 992.1,706.489 C992.1,701.556 996.1,697.556 1001.03,697.556 C1005.96,697.556 1009.96,701.556 1009.96,706.489" id="Fill-1109" sketch:type="MSShapeGroup"></path><path d="M1009.96,733.288 C1009.96,738.221 1005.96,742.221 1001.03,742.221 C996.1,742.221 992.1,738.221 992.1,733.288 C992.1,728.354 996.1,724.355 1001.03,724.355 C1005.96,724.355 1009.96,728.354 1009.96,733.288" id="Fill-1110" sketch:type="MSShapeGroup"></path><path d="M1009.96,760.086 C1009.96,765.02 1005.96,769.019 1001.03,769.019 C996.1,769.019 992.1,765.02 992.1,760.086 C992.1,755.153 996.1,751.154 1001.03,751.154 C1005.96,751.154 1009.96,755.153 1009.96,760.086" id="Fill-1111" sketch:type="MSShapeGroup"></path><path d="M1009.96,786.885 C1009.96,791.818 1005.96,795.818 1001.03,795.818 C996.1,795.818 992.1,791.818 992.1,786.885 C992.1,781.951 996.1,777.952 1001.03,777.952 C1005.96,777.952 1009.96,781.951 1009.96,786.885" id="Fill-1112" sketch:type="MSShapeGroup"></path><path d="M1009.96,813.683 C1009.96,818.617 1005.96,822.616 1001.03,822.616 C996.1,822.616 992.1,818.617 992.1,813.683 C992.1,808.75 996.1,804.751 1001.03,804.751 C1005.96,804.751 1009.96,808.75 1009.96,813.683" id="Fill-1113" sketch:type="MSShapeGroup"></path><path d="M1009.96,840.482 C1009.96,845.415 1005.96,849.415 1001.03,849.415 C996.1,849.415 992.1,845.415 992.1,840.482 C992.1,835.548 996.1,831.549 1001.03,831.549 C1005.96,831.549 1009.96,835.548 1009.96,840.482" id="Fill-1114" sketch:type="MSShapeGroup"></path><path d="M1036.76,90.124 C1036.76,95.057 1032.76,99.057 1027.83,99.057 C1022.89,99.057 1018.89,95.057 1018.89,90.124 C1018.89,85.19 1022.89,81.191 1027.83,81.191 C1032.76,81.191 1036.76,85.19 1036.76,90.124" id="Fill-1115" sketch:type="MSShapeGroup"></path><path d="M1036.76,116.922 C1036.76,121.856 1032.76,125.855 1027.83,125.855 C1022.89,125.855 1018.89,121.856 1018.89,116.922 C1018.89,111.989 1022.89,107.989 1027.83,107.989 C1032.76,107.989 1036.76,111.989 1036.76,116.922" id="Fill-1116" sketch:type="MSShapeGroup"></path><path d="M1036.76,143.721 C1036.76,148.654 1032.76,152.654 1027.83,152.654 C1022.89,152.654 1018.89,148.654 1018.89,143.721 C1018.89,138.787 1022.89,134.788 1027.83,134.788 C1032.76,134.788 1036.76,138.787 1036.76,143.721" id="Fill-1117" sketch:type="MSShapeGroup"></path><path d="M1036.76,170.519 C1036.76,175.453 1032.76,179.452 1027.83,179.452 C1022.89,179.452 1018.89,175.453 1018.89,170.519 C1018.89,165.586 1022.89,161.586 1027.83,161.586 C1032.76,161.586 1036.76,165.586 1036.76,170.519" id="Fill-1118" sketch:type="MSShapeGroup"></path><path d="M1036.76,679.691 C1036.76,684.624 1032.76,688.624 1027.83,688.624 C1022.89,688.624 1018.89,684.624 1018.89,679.691 C1018.89,674.757 1022.89,670.758 1027.83,670.758 C1032.76,670.758 1036.76,674.757 1036.76,679.691" id="Fill-1119" sketch:type="MSShapeGroup"></path><path d="M1036.76,706.489 C1036.76,711.423 1032.76,715.422 1027.83,715.422 C1022.89,715.422 1018.89,711.423 1018.89,706.489 C1018.89,701.556 1022.89,697.556 1027.83,697.556 C1032.76,697.556 1036.76,701.556 1036.76,706.489" id="Fill-1120" sketch:type="MSShapeGroup"></path><path d="M1036.76,733.288 C1036.76,738.221 1032.76,742.221 1027.83,742.221 C1022.89,742.221 1018.89,738.221 1018.89,733.288 C1018.89,728.354 1022.89,724.355 1027.83,724.355 C1032.76,724.355 1036.76,728.354 1036.76,733.288" id="Fill-1121" sketch:type="MSShapeGroup"></path><path d="M1036.76,760.086 C1036.76,765.02 1032.76,769.019 1027.83,769.019 C1022.89,769.019 1018.89,765.02 1018.89,760.086 C1018.89,755.153 1022.89,751.154 1027.83,751.154 C1032.76,751.154 1036.76,755.153 1036.76,760.086" id="Fill-1122" sketch:type="MSShapeGroup"></path><path d="M1036.76,786.885 C1036.76,791.818 1032.76,795.818 1027.83,795.818 C1022.89,795.818 1018.89,791.818 1018.89,786.885 C1018.89,781.951 1022.89,777.952 1027.83,777.952 C1032.76,777.952 1036.76,781.951 1036.76,786.885" id="Fill-1123" sketch:type="MSShapeGroup"></path><path d="M1036.76,813.683 C1036.76,818.617 1032.76,822.616 1027.83,822.616 C1022.89,822.616 1018.89,818.617 1018.89,813.683 C1018.89,808.75 1022.89,804.751 1027.83,804.751 C1032.76,804.751 1036.76,808.75 1036.76,813.683" id="Fill-1124" sketch:type="MSShapeGroup"></path><path d="M1036.76,840.482 C1036.76,845.415 1032.76,849.415 1027.83,849.415 C1022.89,849.415 1018.89,845.415 1018.89,840.482 C1018.89,835.548 1022.89,831.549 1027.83,831.549 C1032.76,831.549 1036.76,835.548 1036.76,840.482" id="Fill-1125" sketch:type="MSShapeGroup"></path><path d="M1036.76,867.28 C1036.76,872.214 1032.76,876.213 1027.83,876.213 C1022.89,876.213 1018.89,872.214 1018.89,867.28 C1018.89,862.347 1022.89,858.348 1027.83,858.348 C1032.76,858.348 1036.76,862.347 1036.76,867.28" id="Fill-1126" sketch:type="MSShapeGroup"></path><path d="M1063.56,90.124 C1063.56,95.057 1059.56,99.057 1054.63,99.057 C1049.69,99.057 1045.69,95.057 1045.69,90.124 C1045.69,85.19 1049.69,81.191 1054.63,81.191 C1059.56,81.191 1063.56,85.19 1063.56,90.124" id="Fill-1127" sketch:type="MSShapeGroup"></path><path d="M1063.56,116.922 C1063.56,121.856 1059.56,125.855 1054.63,125.855 C1049.69,125.855 1045.69,121.856 1045.69,116.922 C1045.69,111.989 1049.69,107.989 1054.63,107.989 C1059.56,107.989 1063.56,111.989 1063.56,116.922" id="Fill-1128" sketch:type="MSShapeGroup"></path><path d="M1063.56,143.721 C1063.56,148.654 1059.56,152.654 1054.63,152.654 C1049.69,152.654 1045.69,148.654 1045.69,143.721 C1045.69,138.787 1049.69,134.788 1054.63,134.788 C1059.56,134.788 1063.56,138.787 1063.56,143.721" id="Fill-1129" sketch:type="MSShapeGroup"></path><path d="M1063.56,170.519 C1063.56,175.453 1059.56,179.452 1054.63,179.452 C1049.69,179.452 1045.69,175.453 1045.69,170.519 C1045.69,165.586 1049.69,161.586 1054.63,161.586 C1059.56,161.586 1063.56,165.586 1063.56,170.519" id="Fill-1130" sketch:type="MSShapeGroup"></path><path d="M1063.56,197.318 C1063.56,202.251 1059.56,206.251 1054.63,206.251 C1049.69,206.251 1045.69,202.251 1045.69,197.318 C1045.69,192.384 1049.69,188.385 1054.63,188.385 C1059.56,188.385 1063.56,192.384 1063.56,197.318" id="Fill-1131" sketch:type="MSShapeGroup"></path><path d="M1063.56,224.116 C1063.56,229.05 1059.56,233.049 1054.63,233.049 C1049.69,233.049 1045.69,229.05 1045.69,224.116 C1045.69,219.183 1049.69,215.184 1054.63,215.184 C1059.56,215.184 1063.56,219.183 1063.56,224.116" id="Fill-1132" sketch:type="MSShapeGroup"></path><path d="M1063.56,250.915 C1063.56,255.848 1059.56,259.848 1054.63,259.848 C1049.69,259.848 1045.69,255.848 1045.69,250.915 C1045.69,245.981 1049.69,241.982 1054.63,241.982 C1059.56,241.982 1063.56,245.981 1063.56,250.915" id="Fill-1133" sketch:type="MSShapeGroup"></path><path d="M1063.56,679.691 C1063.56,684.624 1059.56,688.624 1054.63,688.624 C1049.69,688.624 1045.69,684.624 1045.69,679.691 C1045.69,674.757 1049.69,670.758 1054.63,670.758 C1059.56,670.758 1063.56,674.757 1063.56,679.691" id="Fill-1134" sketch:type="MSShapeGroup"></path><path d="M1063.56,706.489 C1063.56,711.423 1059.56,715.422 1054.63,715.422 C1049.69,715.422 1045.69,711.423 1045.69,706.489 C1045.69,701.556 1049.69,697.556 1054.63,697.556 C1059.56,697.556 1063.56,701.556 1063.56,706.489" id="Fill-1135" sketch:type="MSShapeGroup"></path><path d="M1063.56,733.288 C1063.56,738.221 1059.56,742.221 1054.63,742.221 C1049.69,742.221 1045.69,738.221 1045.69,733.288 C1045.69,728.354 1049.69,724.355 1054.63,724.355 C1059.56,724.355 1063.56,728.354 1063.56,733.288" id="Fill-1136" sketch:type="MSShapeGroup"></path><path d="M1063.56,786.885 C1063.56,791.818 1059.56,795.818 1054.63,795.818 C1049.69,795.818 1045.69,791.818 1045.69,786.885 C1045.69,781.951 1049.69,777.952 1054.63,777.952 C1059.56,777.952 1063.56,781.951 1063.56,786.885" id="Fill-1137" sketch:type="MSShapeGroup"></path><path d="M1063.56,813.683 C1063.56,818.617 1059.56,822.616 1054.63,822.616 C1049.69,822.616 1045.69,818.617 1045.69,813.683 C1045.69,808.75 1049.69,804.751 1054.63,804.751 C1059.56,804.751 1063.56,808.75 1063.56,813.683" id="Fill-1138" sketch:type="MSShapeGroup"></path><path d="M1063.56,840.482 C1063.56,845.415 1059.56,849.415 1054.63,849.415 C1049.69,849.415 1045.69,845.415 1045.69,840.482 C1045.69,835.548 1049.69,831.549 1054.63,831.549 C1059.56,831.549 1063.56,835.548 1063.56,840.482" id="Fill-1139" sketch:type="MSShapeGroup"></path><path d="M1063.56,867.28 C1063.56,872.214 1059.56,876.213 1054.63,876.213 C1049.69,876.213 1045.69,872.214 1045.69,867.28 C1045.69,862.347 1049.69,858.348 1054.63,858.348 C1059.56,858.348 1063.56,862.347 1063.56,867.28" id="Fill-1140" sketch:type="MSShapeGroup"></path><path d="M1063.56,894.079 C1063.56,899.012 1059.56,903.012 1054.63,903.012 C1049.69,903.012 1045.69,899.012 1045.69,894.079 C1045.69,889.145 1049.69,885.146 1054.63,885.146 C1059.56,885.146 1063.56,889.145 1063.56,894.079" id="Fill-1141" sketch:type="MSShapeGroup"></path><path d="M1090.36,90.124 C1090.36,95.057 1086.36,99.057 1081.42,99.057 C1076.49,99.057 1072.49,95.057 1072.49,90.124 C1072.49,85.19 1076.49,81.191 1081.42,81.191 C1086.36,81.191 1090.36,85.19 1090.36,90.124" id="Fill-1142" sketch:type="MSShapeGroup"></path><path d="M1090.36,116.922 C1090.36,121.856 1086.36,125.855 1081.42,125.855 C1076.49,125.855 1072.49,121.856 1072.49,116.922 C1072.49,111.989 1076.49,107.989 1081.42,107.989 C1086.36,107.989 1090.36,111.989 1090.36,116.922" id="Fill-1143" sketch:type="MSShapeGroup"></path><path d="M1090.36,143.721 C1090.36,148.654 1086.36,152.654 1081.42,152.654 C1076.49,152.654 1072.49,148.654 1072.49,143.721 C1072.49,138.787 1076.49,134.788 1081.42,134.788 C1086.36,134.788 1090.36,138.787 1090.36,143.721" id="Fill-1144" sketch:type="MSShapeGroup"></path><path d="M1090.36,170.519 C1090.36,175.453 1086.36,179.452 1081.42,179.452 C1076.49,179.452 1072.49,175.453 1072.49,170.519 C1072.49,165.586 1076.49,161.586 1081.42,161.586 C1086.36,161.586 1090.36,165.586 1090.36,170.519" id="Fill-1145" sketch:type="MSShapeGroup"></path><path d="M1090.36,197.318 C1090.36,202.251 1086.36,206.251 1081.42,206.251 C1076.49,206.251 1072.49,202.251 1072.49,197.318 C1072.49,192.384 1076.49,188.385 1081.42,188.385 C1086.36,188.385 1090.36,192.384 1090.36,197.318" id="Fill-1146" sketch:type="MSShapeGroup"></path><path d="M1090.36,786.885 C1090.36,791.818 1086.36,795.818 1081.42,795.818 C1076.49,795.818 1072.49,791.818 1072.49,786.885 C1072.49,781.951 1076.49,777.952 1081.42,777.952 C1086.36,777.952 1090.36,781.951 1090.36,786.885" id="Fill-1147" sketch:type="MSShapeGroup"></path><path d="M1090.36,813.683 C1090.36,818.617 1086.36,822.616 1081.42,822.616 C1076.49,822.616 1072.49,818.617 1072.49,813.683 C1072.49,808.75 1076.49,804.751 1081.42,804.751 C1086.36,804.751 1090.36,808.75 1090.36,813.683" id="Fill-1148" sketch:type="MSShapeGroup"></path><path d="M1090.36,840.482 C1090.36,845.415 1086.36,849.415 1081.42,849.415 C1076.49,849.415 1072.49,845.415 1072.49,840.482 C1072.49,835.548 1076.49,831.549 1081.42,831.549 C1086.36,831.549 1090.36,835.548 1090.36,840.482" id="Fill-1149" sketch:type="MSShapeGroup"></path><path d="M1090.36,867.28 C1090.36,872.214 1086.36,876.213 1081.42,876.213 C1076.49,876.213 1072.49,872.214 1072.49,867.28 C1072.49,862.347 1076.49,858.348 1081.42,858.348 C1086.36,858.348 1090.36,862.347 1090.36,867.28" id="Fill-1150" sketch:type="MSShapeGroup"></path><path d="M1117.16,90.124 C1117.16,95.057 1113.16,99.057 1108.22,99.057 C1103.29,99.057 1099.29,95.057 1099.29,90.124 C1099.29,85.19 1103.29,81.191 1108.22,81.191 C1113.16,81.191 1117.16,85.19 1117.16,90.124" id="Fill-1151" sketch:type="MSShapeGroup"></path><path d="M1117.16,116.922 C1117.16,121.856 1113.16,125.855 1108.22,125.855 C1103.29,125.855 1099.29,121.856 1099.29,116.922 C1099.29,111.989 1103.29,107.989 1108.22,107.989 C1113.16,107.989 1117.16,111.989 1117.16,116.922" id="Fill-1152" sketch:type="MSShapeGroup"></path><path d="M1117.16,143.721 C1117.16,148.654 1113.16,152.654 1108.22,152.654 C1103.29,152.654 1099.29,148.654 1099.29,143.721 C1099.29,138.787 1103.29,134.788 1108.22,134.788 C1113.16,134.788 1117.16,138.787 1117.16,143.721" id="Fill-1153" sketch:type="MSShapeGroup"></path><path d="M1117.16,170.519 C1117.16,175.453 1113.16,179.452 1108.22,179.452 C1103.29,179.452 1099.29,175.453 1099.29,170.519 C1099.29,165.586 1103.29,161.586 1108.22,161.586 C1113.16,161.586 1117.16,165.586 1117.16,170.519" id="Fill-1154" sketch:type="MSShapeGroup"></path><path d="M1143.95,90.124 C1143.95,95.057 1139.95,99.057 1135.02,99.057 C1130.09,99.057 1126.09,95.057 1126.09,90.124 C1126.09,85.19 1130.09,81.191 1135.02,81.191 C1139.95,81.191 1143.95,85.19 1143.95,90.124" id="Fill-1155" sketch:type="MSShapeGroup"></path><path d="M1143.95,116.922 C1143.95,121.856 1139.95,125.855 1135.02,125.855 C1130.09,125.855 1126.09,121.856 1126.09,116.922 C1126.09,111.989 1130.09,107.989 1135.02,107.989 C1139.95,107.989 1143.95,111.989 1143.95,116.922" id="Fill-1156" sketch:type="MSShapeGroup"></path><path d="M1143.95,143.721 C1143.95,148.654 1139.95,152.654 1135.02,152.654 C1130.09,152.654 1126.09,148.654 1126.09,143.721 C1126.09,138.787 1130.09,134.788 1135.02,134.788 C1139.95,134.788 1143.95,138.787 1143.95,143.721" id="Fill-1157" sketch:type="MSShapeGroup"></path><path d="M1143.95,170.519 C1143.95,175.453 1139.95,179.452 1135.02,179.452 C1130.09,179.452 1126.09,175.453 1126.09,170.519 C1126.09,165.586 1130.09,161.586 1135.02,161.586 C1139.95,161.586 1143.95,165.586 1143.95,170.519" id="Fill-1158" sketch:type="MSShapeGroup"></path><path d="M1143.95,733.288 C1143.95,738.221 1139.95,742.221 1135.02,742.221 C1130.09,742.221 1126.09,738.221 1126.09,733.288 C1126.09,728.354 1130.09,724.355 1135.02,724.355 C1139.95,724.355 1143.95,728.354 1143.95,733.288" id="Fill-1159" sketch:type="MSShapeGroup"></path><path d="M1170.75,63.325 C1170.75,68.259 1166.75,72.258 1161.82,72.258 C1156.89,72.258 1152.89,68.259 1152.89,63.325 C1152.89,58.392 1156.89,54.392 1161.82,54.392 C1166.75,54.392 1170.75,58.392 1170.75,63.325" id="Fill-1160" sketch:type="MSShapeGroup"></path><path d="M1170.75,90.124 C1170.75,95.057 1166.75,99.057 1161.82,99.057 C1156.89,99.057 1152.89,95.057 1152.89,90.124 C1152.89,85.19 1156.89,81.191 1161.82,81.191 C1166.75,81.191 1170.75,85.19 1170.75,90.124" id="Fill-1161" sketch:type="MSShapeGroup"></path><path d="M1170.75,116.922 C1170.75,121.856 1166.75,125.855 1161.82,125.855 C1156.89,125.855 1152.89,121.856 1152.89,116.922 C1152.89,111.989 1156.89,107.989 1161.82,107.989 C1166.75,107.989 1170.75,111.989 1170.75,116.922" id="Fill-1162" sketch:type="MSShapeGroup"></path><path d="M1170.75,143.721 C1170.75,148.654 1166.75,152.654 1161.82,152.654 C1156.89,152.654 1152.89,148.654 1152.89,143.721 C1152.89,138.787 1156.89,134.788 1161.82,134.788 C1166.75,134.788 1170.75,138.787 1170.75,143.721" id="Fill-1163" sketch:type="MSShapeGroup"></path><path d="M1170.75,170.519 C1170.75,175.453 1166.75,179.452 1161.82,179.452 C1156.89,179.452 1152.89,175.453 1152.89,170.519 C1152.89,165.586 1156.89,161.586 1161.82,161.586 C1166.75,161.586 1170.75,165.586 1170.75,170.519" id="Fill-1164" sketch:type="MSShapeGroup"></path><path d="M1170.75,733.288 C1170.75,738.221 1166.75,742.221 1161.82,742.221 C1156.89,742.221 1152.89,738.221 1152.89,733.288 C1152.89,728.354 1156.89,724.355 1161.82,724.355 C1166.75,724.355 1170.75,728.354 1170.75,733.288" id="Fill-1165" sketch:type="MSShapeGroup"></path><path d="M1170.75,760.086 C1170.75,765.02 1166.75,769.019 1161.82,769.019 C1156.89,769.019 1152.89,765.02 1152.89,760.086 C1152.89,755.153 1156.89,751.154 1161.82,751.154 C1166.75,751.154 1170.75,755.153 1170.75,760.086" id="Fill-1166" sketch:type="MSShapeGroup"></path><path d="M1197.55,63.325 C1197.55,68.259 1193.55,72.258 1188.62,72.258 C1183.69,72.258 1179.69,68.259 1179.69,63.325 C1179.69,58.392 1183.69,54.392 1188.62,54.392 C1193.55,54.392 1197.55,58.392 1197.55,63.325" id="Fill-1167" sketch:type="MSShapeGroup"></path><path d="M1197.55,90.124 C1197.55,95.057 1193.55,99.057 1188.62,99.057 C1183.69,99.057 1179.69,95.057 1179.69,90.124 C1179.69,85.19 1183.69,81.191 1188.62,81.191 C1193.55,81.191 1197.55,85.19 1197.55,90.124" id="Fill-1168" sketch:type="MSShapeGroup"></path><path d="M1197.55,116.922 C1197.55,121.856 1193.55,125.855 1188.62,125.855 C1183.69,125.855 1179.69,121.856 1179.69,116.922 C1179.69,111.989 1183.69,107.989 1188.62,107.989 C1193.55,107.989 1197.55,111.989 1197.55,116.922" id="Fill-1169" sketch:type="MSShapeGroup"></path><path d="M1197.55,143.721 C1197.55,148.654 1193.55,152.654 1188.62,152.654 C1183.69,152.654 1179.69,148.654 1179.69,143.721 C1179.69,138.787 1183.69,134.788 1188.62,134.788 C1193.55,134.788 1197.55,138.787 1197.55,143.721" id="Fill-1170" sketch:type="MSShapeGroup"></path><path d="M1197.55,170.519 C1197.55,175.453 1193.55,179.452 1188.62,179.452 C1183.69,179.452 1179.69,175.453 1179.69,170.519 C1179.69,165.586 1183.69,161.586 1188.62,161.586 C1193.55,161.586 1197.55,165.586 1197.55,170.519" id="Fill-1171" sketch:type="MSShapeGroup"></path><path d="M1197.55,920.88 C1197.55,925.81 1193.55,929.81 1188.62,929.81 C1183.69,929.81 1179.69,925.81 1179.69,920.88 C1179.69,915.944 1183.69,911.944 1188.62,911.944 C1193.55,911.944 1197.55,915.944 1197.55,920.88" id="Fill-1172" sketch:type="MSShapeGroup"></path><path d="M1197.55,947.68 C1197.55,952.61 1193.55,956.61 1188.62,956.61 C1183.69,956.61 1179.69,952.61 1179.69,947.68 C1179.69,942.74 1183.69,938.74 1188.62,938.74 C1193.55,938.74 1197.55,942.74 1197.55,947.68" id="Fill-1173" sketch:type="MSShapeGroup"></path><path d="M1224.35,840.482 C1224.35,845.415 1220.35,849.415 1215.42,849.415 C1210.48,849.415 1206.48,845.415 1206.48,840.482 C1206.48,835.548 1210.48,831.549 1215.42,831.549 C1220.35,831.549 1224.35,835.548 1224.35,840.482" id="Fill-1174" sketch:type="MSShapeGroup"></path><path d="M1224.35,867.28 C1224.35,872.214 1220.35,876.213 1215.42,876.213 C1210.48,876.213 1206.48,872.214 1206.48,867.28 C1206.48,862.347 1210.48,858.348 1215.42,858.348 C1220.35,858.348 1224.35,862.347 1224.35,867.28" id="Fill-1175" sketch:type="MSShapeGroup"></path><path d="M1224.35,894.079 C1224.35,899.012 1220.35,903.012 1215.42,903.012 C1210.48,903.012 1206.48,899.012 1206.48,894.079 C1206.48,889.145 1210.48,885.146 1215.42,885.146 C1220.35,885.146 1224.35,889.145 1224.35,894.079" id="Fill-1176" sketch:type="MSShapeGroup"></path><path d="M1224.35,920.88 C1224.35,925.81 1220.35,929.81 1215.42,929.81 C1210.48,929.81 1206.48,925.81 1206.48,920.88 C1206.48,915.944 1210.48,911.944 1215.42,911.944 C1220.35,911.944 1224.35,915.944 1224.35,920.88" id="Fill-1177" sketch:type="MSShapeGroup"></path><path d="M1251.15,867.28 C1251.15,872.214 1247.15,876.213 1242.22,876.213 C1237.28,876.213 1233.28,872.214 1233.28,867.28 C1233.28,862.347 1237.28,858.348 1242.22,858.348 C1247.15,858.348 1251.15,862.347 1251.15,867.28" id="Fill-1178" sketch:type="MSShapeGroup"></path><path d="M1251.15,894.079 C1251.15,899.012 1247.15,903.012 1242.22,903.012 C1237.28,903.012 1233.28,899.012 1233.28,894.079 C1233.28,889.145 1237.28,885.146 1242.22,885.146 C1247.15,885.146 1251.15,889.145 1251.15,894.079" id="Fill-1179" sketch:type="MSShapeGroup"></path></g></g></g></svg>';

// Legacy - Support modal
$(document).ready(function () {
  var submitTicketLink = $('a[href="#submit-a-ticket"]');
  var supportModal = $('#support-modal');
  var supportModalOverlay = $('#support-modal .overlay');
  var supportModalClose = $('#support-modal .close');
  var emailSuggestionBox = $('#email-message-suggestion');

  submitTicketLink.click(openModal);
  supportModalOverlay.click(closeModal);
  supportModalClose.click(closeModal);

  function closeModal (event) {
    supportModal.removeClass('show');
    $("html").css("overflow", "scroll");
    $("body").css("position", "static");
  }

  function openModal (event) {
    event.preventDefault();
    supportModal.addClass('show');
    $("html").css("overflow", "hidden")
    $("body").css("position", "fixed")
  }
  /*
   * Keyboard shortcuts
   */
  $(document).keyup(function(e) {
    // esc key
    if (e.keyCode == 27) {
      closeModal()
    }
  });
  /*
   * Email suggestions
   */
  var currentSuggestion = null;
  emailSuggestionBox.click(function (e) {
    if (emailSuggestionBox.html().length > 1) {
      $('#email').val(currentSuggestion.full);
      emailSuggestionBox.html('');
    }
  });

  $('#email').on('blur', function() {
    $(this).mailcheck({
      suggested: function(element, suggestion) {
        currentSuggestion = suggestion;
        emailSuggestionBox.html('Did you mean <span>' + suggestion.full + '</span>?');
      },
      empty: function(element) {
        emailSuggestionBox.html('');
      }
    });
  });

    function o(o) {
        var t = $("#send-button");
        t.html("Sending"); {
            var n = "https://api.acorns.com/v1/user/download";
            $.post(n, {
                number: o
            }, function() {}).done(function(o) {
                log("response:", o), t.html("Resend"), $(".phone-icon").addClass("activate")
            }).fail(function(o) {
                log(o), alert("We're sorry, an error occured. Try again shortly.")
            }).always(function() {})
        }
    }

    function t(o) {
        return o.replace(/[^\d]+/gi, "").length >= 7
    }

    function n(n) {
        n.preventDefault();
        var e = v.val();
        log("number:", e), t(e) && o(e)
    }

    function e() {
        $.post("https://api.acorns.com/proxy/sales_force", $("form#submitCase").serialize(), function() {})
    }

    function i(o) {
        return "" == $(o).val() ? !1 : !0
    }

    function a(o, t) {
        return $("#topic").text() !== o ? !0 : $("#topic").text() == o && $(t).val().length >= 4 ? !0 : !1
    }

    function c() {
        return 1 == i("#firstname") && 1 == i("#lastname") && 1 == i("textarea") && 1 == r() && 1 == s() && "Select a Topic" !== $("#topic").text() && 1 == a("Other", "#other") && 1 == a("Email Correction", "#corrected-email") && void 0 !== $("form input[type=radio]:checked").val() ? ($(".errors").slideUp(250), !0) : !1
    }

    function r() {
        var o = $("#email").val(),
            t = l(o);
        return t && 1 == i("#email") ? !0 : !1
    }

    function s() {
        var o = $("#phone").val(),
            t = d(o);
        return t && 1 == i("#phone") ? !0 : !1
    }

    function l(o) {
        var t = new RegExp(/^[+a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/i);
        return t.test(o)
    }

    function d(o) {
        var t = new RegExp(/^(\+0?1\s)?\(?\d{3}\)?[\s.-]\d{3}[\s.-]\d{4}$/i);
        return t.test(o)
    }

    function u() {
        "Forgotten PIN or Password" == $("#topic").text() ? ($("#type").val("Other"), $("#subject").val("Forgotten PIN or Password")) : $("#type, #subject").val($("#topic").text())
    }
    var m = 0,
        f = {
            Android: function() {
                return navigator.userAgent.match(/Android/i)
            },
            iOS: function() {
                return navigator.userAgent.match(/iPhone|iPad|iPod/i)
            }
        },
        p = $("#download-app");

    var h = $("#submit-a-ticket");
    $("#submit-a-ticket").on("click", function(o) {
        o.stopPropagation()
    }), $(".download-app-modal .row.frame, .submit-a-ticket .row.frame, .live-chat-modal .row.frame").on("click", function(o) {
        o.stopPropagation()
    }), $(".download-app-modal").on("click", function() {
        var o = $(this);
        o.addClass("fadeOut"), setTimeout(function() {
            o.removeClass("fadeOut"), p.modal("hide"), h.modal("hide");
            var t = document.body.scrollTop;
            window.location.hash = " ", document.body.scrollTop = t
        }, 300)
    }), $(document).keyup(function(o) {
        27 == o.keyCode && m >= 7 && (document.location.href = "#")
    });
    var v = $("#phone-number");
    v.mask("+1 (999) 999-9999");
    var w = "iOS";
    /android/i.test(navigator.userAgent) && (w = "Android"), $("#send-button").on("click", n), $(".sms-form").on("submit", n), $("#submitCase button").click(function(o) {
        if (o.preventDefault(), 1 == c()) {
            var t = $("#firstname").val() + " " + $("#lastname").val();
            $("#name").val(t), $("#url").val(window.location.pathname), $("#browser-size").val($(window).width().toString() + " x " + $(window).height().toString() + "px"), $("#user-agent").val(navigator.userAgent), $("#device").val(navigator.platform), u(), e(), $("#submitCase").addClass("animated fadeOutDown"), $("#submit-a-ticket").scrollTop(0), $(".confirmation-message").css("display", "inline-block").addClass("animated fadeInUp"), $("#submit-a-ticket").css("overflow", "hidden")
        } else $(".errors").slideDown(250)
    }), $("form#submitCase").bind("click keyup", function() {
        1 == c() ? $("button").removeClass("disabled") : $("button").addClass("disabled")
    });
    var b = $("#phone");
    b.mask("+1 (999) 999-9999");
    var g = $("#dob");
    g.mask("99/99/9999"), $(".topic-container").click(function(o) {
        o.stopPropagation(), $(this).addClass("lime-inner-glow"), $(".topic-list").show()
    }), $("#submitCase, .modal").click(function() {
        $(".topic-list").hide(), $(".topic-container").removeClass("lime-inner-glow")
    }), $(".topic-list ul li").click(function() {
        $(".topic-container").text($(this).text()), "Other" == $(".topic-container").text() ? ($(".other").show(), $("#other").focus()) : $(".other").hide(), "Email Correction" == $(".topic-container").text() ? ($(".email-correction, li.account-info").show(), $("#corrected-email").focus()) : "Forgotten PIN or Password" == $(".topic-container").text() ? ($(".email-correction").hide(), $("li.account-info").show(), $("#ssn").focus()) : $(".email-correction, li.account-info").hide(), "Cancel My Order" == $(".topic-container").text() ? $(".cancel-my-order").show() : $(".cancel-my-order").hide(), "Close My Account" == $('.topic-container').text() ? $('.name-info, .contact-info, textarea, .platform, button').hide() && $('.close-my-account-button').css('display', 'inline-block') : $('.name-info, .contact-info, textarea, .platform, button').show() && $('.close-my-account-button').hide(), $(".topic-list").hide(), $(".topic-container").removeClass("lime-inner-glow")
    }), $(".confirmation-message .btn").on("click", closeModal);
});

jQuery(document).ready(function () {
    var queryString = window.location.href.slice(window.location.href.indexOf('?'));
    jQuery('a[href$="/registration/c/start"').each(function() {
        var href = jQuery(this).attr('href');
        jQuery(this).attr('href', href + queryString);
    });

    jQuery('a[href^="/"').each(function() {
        var href = jQuery(this).attr('href');
        jQuery(this).attr('href', href + queryString);
    });
});
